use std::collections::HashSet;

use bluster::{gatt::characteristic::{Characteristic, Properties}, SdpShortUuid};
use futures::channel::mpsc::channel;
use uuid::Uuid;
use crate::TEMPERATURE_CHARACTERISTIC_UUID;

pub fn make_temperature_characteristic() -> Characteristic {
    Characteristic::new(
        Uuid::from_sdp_short_uuid(TEMPERATURE_CHARACTERISTIC_UUID),
        make_properties(),
        None,
        HashSet::new()
    )
}

fn make_properties() -> Properties {
    Properties::new(
        None,
        None,
        None,
        None
    )
}