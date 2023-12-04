use std::collections::HashSet;

use bluster::{gatt::service::Service, SdpShortUuid};
use uuid::Uuid;

use crate::SERVICE_UUID;

use crate::gatt::temperature::TemperatureCharacteristic;

use self::characteristic::CharacteristicCreator;

mod characteristic;
mod temperature;

pub async fn create_service() -> (Uuid, Service) {
    let service_uuid = Uuid::from_sdp_short_uuid(SERVICE_UUID);

    let mut characteristics = HashSet::new();
    characteristics.insert(TemperatureCharacteristic::create_characteristic());

    (
        service_uuid,
        Service::new(service_uuid, true, characteristics),
    )
}
