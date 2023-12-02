use std::{collections::HashSet, sync::Arc, time::Duration};

use tokio::{sync::Mutex, runtime::Runtime};
use uuid::Uuid;
use anyhow::{Result, Context};
use tracing::{debug, info};
use bluster::{SdpShortUuid, Peripheral, gatt::service::Service};
use lazy_static::lazy_static;

use crate::gatt::create_service;

mod gatt;

pub const TEMPERATURE_CHARACTERISTIC_UUID: u16 = 0x2A3D;
pub const SERVICE_UUID: u16 = 0x2A3D;
const ADVERTISE_NAME: &str = "SOME_NAME";

lazy_static! {
    pub static ref RUNTIME: Arc<Mutex<Runtime>> = Arc::new(Mutex::new(Runtime::new().context("Failed creating a tokio runtime").unwrap()));
}

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt().with_max_level(tracing::Level::DEBUG).init();

    let peripheral = make_peripheral().await.unwrap();

    let runtime = RUNTIME.clone();
    let runtime = runtime.lock().await;

    runtime.spawn(async move {
        info!("Powering on peripheral");

        while !peripheral.is_powered().await.unwrap() {};

        peripheral.start_advertising(ADVERTISE_NAME, &[]).await.context("Failed to advertise").unwrap();
        info!("Start advertising {}", ADVERTISE_NAME);

        // Wait while peripheral is advertising
        while peripheral.is_advertising().await.unwrap() {
        
        }
        info!("Peripheral stopped advertising");
    });

    loop {
        // TODO: make ctrl-c work
        std::thread::sleep(Duration::from_millis(250));
    }
}

async fn make_peripheral() -> Result<Peripheral> {
    debug!("Starting to make peripheral");
    let peripheral = Peripheral::new().await.context("Could not make peripheral")?;

    debug!("Adding service");
    peripheral.add_service(&create_service().await).context("Could not add service to peripheral")?;

    debug!("Powering the peripheral");
    while !peripheral.is_powered().await? { }

    debug!("Registering gatt");
    peripheral.register_gatt().await.context("Failed to register gatt service")?;

    Ok(peripheral)
}

fn make_service() -> (Uuid, Service) {
    let mut characteristics = HashSet::new();
    // characteristics.insert(make_temperature_characteristic());
    let uuid = Uuid::from_sdp_short_uuid(SERVICE_UUID);

    (uuid, Service::new(uuid, true, characteristics))
}
