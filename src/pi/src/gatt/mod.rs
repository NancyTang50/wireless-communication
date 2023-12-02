use std::collections::HashSet;

use bluster::{gatt::{event::{Event, Response}, descriptor::{Descriptor, Properties, Read, Secure}, service::Service}, SdpShortUuid};
use futures::{channel::mpsc::channel, StreamExt};
use tracing::debug;
use uuid::Uuid;

use crate::SERVICE_UUID;

use self::temperature::create_temperature;

mod temperature;

pub async fn create_service() -> (Uuid, Service) {
    let service_uuid = Uuid::from_sdp_short_uuid(SERVICE_UUID);

    let mut characteristics = HashSet::new();
    characteristics.insert(create_temperature().await);

    (service_uuid, Service::new(service_uuid, true, characteristics))
}

pub async fn create_gatt_description(description: impl ToString) -> Descriptor {
    let (tx, mut rx) = channel(1);

    let value = Vec::from(description.to_string());
    let value2 = value.clone();

    tokio::spawn(async move {
        loop {
            if let Some(event) = rx.next().await {
                debug!("Characteristic_user_description event: {:?}", event);
                if let Event::ReadRequest(read_request) = event {
                    read_request.response.send(Response::Success(value.clone())).unwrap();
                }
            }
        }
    });

    Descriptor::new(
        Uuid::from_sdp_short_uuid(0x2901_u16), // org.bluetooth.descriptor.gatt.characteristic_user_description
        Properties::new(Some(Read(Secure::Insecure(tx))), None),
        Some(value2),
    )
}

pub async fn create_gatt_characteristic_presentation_format(format: u8, exponent: u8, unit: u16, namespace: u8, description: u16) -> Descriptor {
    let (tx, mut rx) = channel(1);
    let [unit_hi, unit_low] = unit.to_le_bytes();
    let [description_hi, description_low] = description.to_le_bytes();

    let value = vec![
        format,
        exponent,
        unit_hi,
        unit_low,
        namespace,
        description_hi,
        description_low,
    ];
    let value2 = value.clone();

    tokio::spawn(async move {
        loop {
            if let Some(event) = rx.next().await {
                debug!("Characteristic_presentation_format event: {:?}", event);
                if let Event::ReadRequest(read_request) = event {
                    read_request.response.send(Response::Success(value.clone())).unwrap();
                }
            }
        }
    });

    Descriptor::new(
        Uuid::from_sdp_short_uuid(0x2901_u16), // org.bluetooth.descriptor.gatt.characteristic_presentation_format
        Properties::new(Some(Read(Secure::Insecure(tx))), None),
        Some(value2),
    )
}