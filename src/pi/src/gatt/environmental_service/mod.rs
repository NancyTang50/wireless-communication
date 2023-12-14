mod humidity_characteristic;
mod temperature_characteristic;

use futures::channel::mpsc::channel;
pub use humidity_characteristic::HumidityCharacteristic;
pub use temperature_characteristic::TemperatureCharacteristic;

use bluster::{gatt::service::Service, SdpShortUuid};
use std::collections::HashSet;
use uuid::Uuid;

use crate::{sensor_data::start_reading_sensor_data, ENVIRONMENTAL_SENSING_SERVICE_UUID};

pub async fn create_evironmental_service() -> (Uuid, Service) {
    let service_uuid = Uuid::from_sdp_short_uuid(ENVIRONMENTAL_SENSING_SERVICE_UUID);

    let (temperature_tx, temperature_rx) = channel(0);
    let (humidity_tx, humidity_rx) = channel(0);

    start_reading_sensor_data(temperature_tx, humidity_tx);

    let mut characteristics = HashSet::new();
    characteristics.insert(TemperatureCharacteristic::create_characteristic(
        temperature_rx,
    ));
    characteristics.insert(HumidityCharacteristic::create_characteristic(humidity_rx));

    (
        service_uuid,
        Service::new(service_uuid, true, characteristics),
    )
}
