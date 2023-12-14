use std::collections::HashSet;

use bluster::{gatt::service::Service, SdpShortUuid};
use uuid::Uuid;

use crate::CURRENT_TIME_SERVICE_UUID;

use self::current_time_characteristic::CurrentTimeCharacteristic;

mod current_time_characteristic;

pub async fn create_current_time_service() -> (Uuid, Service) {
    let service_uuid = Uuid::from_sdp_short_uuid(CURRENT_TIME_SERVICE_UUID);

    let mut characteristics = HashSet::new();
    characteristics.insert(CurrentTimeCharacteristic::create_characteristic());

    (
        service_uuid,
        Service::new(service_uuid, false, characteristics),
    )
}
