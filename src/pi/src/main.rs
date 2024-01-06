use std::time::Duration;

use anyhow::{Context, Result};
use bluster::Peripheral;
use time::{macros::format_description, UtcOffset};
use tracing::{debug, info, Level};
use tracing_subscriber::fmt::time::OffsetTime;
use uuid::Uuid;

use crate::gatt::{create_current_time_service, create_evironmental_service};

mod ble_date_time;
mod ble_encode;
mod gatt;
mod sensor_data;

// NOTE: https://www.bluetooth.com/wp-content/uploads/Files/Specification/Assigned_Numbers.pdf
pub const TEMPERATURE_CHARACTERISTIC_UUID: u16 = 0x2A6E;
pub const HUMIDITY_CHARACTERISTIC_UUID: u16 = 0x2A6F;
pub const ENVIRONMENTAL_SENSING_SERVICE_UUID: u16 = 0x181A;
pub const CURRENT_TIME_SERVICE_UUID: u16 = 0x1805;
pub const CURRENT_TIME_CHARACTERISTIC_UUID: u16 = 0x2A2B;
const ADVERTISE_NAME: &str = "RoomSensor-PI";

/// The max level of logging in debug mode
#[cfg(debug_assertions)]
const TRACING_LEVEL: Level = Level::DEBUG;
/// The max level of logging in release mode
#[cfg(not(debug_assertions))]
const TRACING_LEVEL: Level = Level::INFO;

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt()
        .compact()
        .with_timer(time_formatter())
        .with_max_level(TRACING_LEVEL)
        .init();

    let (services, peripheral) = make_peripheral().await.unwrap();

    info!("Powering on peripheral");

    while !peripheral.is_powered().await.unwrap() {}

    peripheral
        .start_advertising(ADVERTISE_NAME, &services)
        .await
        .context("Failed to advertise")
        .unwrap();
    info!("Start advertising {}", ADVERTISE_NAME);

    // Wait while peripheral is advertising
    while peripheral.is_advertising().await.unwrap() {
        std::thread::sleep(Duration::from_millis(250));
    }
    info!("Peripheral stopped advertising");

    Ok(())
}

async fn make_peripheral() -> Result<(Vec<Uuid>, Peripheral)> {
    debug!("Starting to make peripheral");
    let peripheral = Peripheral::new()
        .await
        .context("Could not make peripheral")?;

    debug!("Adding service");
    let (environmental_service_uuid, service) = create_evironmental_service().await;
    peripheral
        .add_service(&service)
        .context("Could not add service to peripheral")?;
    let (current_time_service_uuid, service) = create_current_time_service().await;
    peripheral
        .add_service(&service)
        .context("Could not add service to peripheral")?;

    debug!("Powering the peripheral");
    while !peripheral.is_powered().await? {}

    debug!("Registering gatt");
    peripheral
        .register_gatt()
        .await
        .context("Failed to register gatt service")?;

    Ok((
        vec![environmental_service_uuid, current_time_service_uuid],
        peripheral,
    ))
}

fn time_formatter() -> OffsetTime<&'static [time::format_description::FormatItem<'static>]> {
    let timer = format_description!("[hour]:[minute]:[second]");
    let time_offset = UtcOffset::current_local_offset().unwrap_or(UtcOffset::UTC);
    OffsetTime::new(time_offset, timer)
}
