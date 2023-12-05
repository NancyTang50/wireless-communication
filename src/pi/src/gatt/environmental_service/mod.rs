mod humidity_characteristic;
mod temperature_characteristic;

use futures::channel::oneshot::channel;
pub use humidity_characteristic::HumidityCharacteristic;
pub use temperature_characteristic::TemperatureCharacteristic;

use std::collections::HashSet;

use bluster::{gatt::service::Service, SdpShortUuid};
use uuid::Uuid;

use crate::{SERVICE_UUID, sensor_data::start_reading_sensor_data};

pub async fn create_evironmental_service() -> (Uuid, Service) {
    let service_uuid = Uuid::from_sdp_short_uuid(SERVICE_UUID);

    let (temperature_rx, _temperature_tx) = channel();
    let (humidity_rx, _humidity_tx) = channel();

    start_reading_sensor_data(temperature_rx, humidity_rx);

    let mut characteristics = HashSet::new();
    characteristics.insert(TemperatureCharacteristic::create_characteristic());
    characteristics.insert(HumidityCharacteristic::create_characteristic());

    (
        service_uuid,
        Service::new(service_uuid, true, characteristics),
    )
}

