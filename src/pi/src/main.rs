use btleplug::{api::{bleuuid::uuid_from_u16, Manager as _, Central, ScanFilter, CentralEvent}, platform::{Manager, Adapter}};
use uuid::Uuid;
use futures::stream::StreamExt;
use anyhow::{Result, Context};
use tracing::debug;

const TEMPERATURE_CHARACTERISTIC_UUID : Uuid = uuid_from_u16(0xFFE9);
const HUMIDITY_CHARACTERISTIC_UUID : Uuid = uuid_from_u16(0xFFE9);

async fn get_central(manager: &Manager) -> Result<Option<Adapter>> {
    let adapters = manager.adapters().await?;
    Ok(adapters.into_iter().next())
}

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt::init();

    debug!("Temperature uuid {}", TEMPERATURE_CHARACTERISTIC_UUID);
    debug!("Humidity uuid {}", HUMIDITY_CHARACTERISTIC_UUID);

    let manager = Manager::new().await.context("The BLE Manager could not create a new instance")?;

    let central = get_central(&manager).await?.expect("Some adapter from the manager");

    let mut events = central.events().await?;

    central.start_scan(ScanFilter::default()).await?;

    while let Some(event) = events.next().await {
        match event {
            CentralEvent::DeviceDiscovered(id) => {
                debug!("DeviceDiscoverd: {:?}", id);
            }
            CentralEvent::DeviceUpdated(id) => {
                debug!("DeviceUpdated: {:?}", id);
            }
            CentralEvent::DeviceConnected(id) => {
                debug!("DeviceConnected: {:?}", id);
            }
            CentralEvent::DeviceDisconnected(id) => {
                debug!("DeviceDisconnected: {:?}", id);
            }
            CentralEvent::ManufacturerDataAdvertisement { id, manufacturer_data } => {
                debug!("ManufacturerDataAdvertisement: {:?} {:?}", id, manufacturer_data);
            }
            CentralEvent::ServiceDataAdvertisement { id, service_data } => {
                debug!("ServiceDataAdvertisement: {:?} {:?}", id, service_data);
            }
            CentralEvent::ServicesAdvertisement { id, services } => {
                debug!("ServicesAdvertisement: {:?} {:?}", id, services);
            }
        }
    }

    Ok(())
}
