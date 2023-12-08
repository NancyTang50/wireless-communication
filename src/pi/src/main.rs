use std::time::Duration;

use anyhow::{Context, Result};
use bluster::Peripheral;
use tracing::{debug, info};
use uuid::Uuid;

use crate::gatt::create_evironmental_service;

mod ble_encode;
mod sensor_data;
mod gatt;

// NOTE: https://www.bluetooth.com/wp-content/uploads/Files/Specification/Assigned_Numbers.pdf
pub const TEMPERATURE_CHARACTERISTIC_UUID: u16 = 0x2A6E;
pub const HUMIDITY_CHARACTERISTIC_UUID: u16 = 0x2A6F;
pub const SERVICE_UUID: u16 = 0x181A;
const ADVERTISE_NAME: &str = "SOME_NAME";

#[tokio::main(flavor = "multi_thread", worker_threads = 4)]
async fn main() -> Result<()> {
    tracing_subscriber::fmt()
        .with_max_level(tracing::Level::DEBUG)
        .init();

    let (service_uuid, peripheral) = make_peripheral().await.unwrap();

    info!("Powering on peripheral");

    while !peripheral.is_powered().await.unwrap() {}

    peripheral
        .start_advertising(ADVERTISE_NAME, &[service_uuid])
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

async fn make_peripheral() -> Result<(Uuid, Peripheral)> {
    debug!("Starting to make peripheral");
    let peripheral = Peripheral::new()
        .await
        .context("Could not make peripheral")?;

    debug!("Adding service");
    let (service_uuid, service) = create_evironmental_service().await;
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

    Ok((service_uuid, peripheral))
}
