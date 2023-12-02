use std::collections::HashSet;

use bluster::{gatt::{event::{Event, Response}, descriptor::{Descriptor, Properties, Read, Secure}, service::Service}, SdpShortUuid};
use futures::{channel::mpsc::channel, StreamExt};
use uuid::Uuid;

use crate::{RUNTIME, SERVICE_UUID};

use self::temperature::create_temperature;

mod temperature;

pub async fn create_service() -> Service {
    let service_uuid = Uuid::from_sdp_short_uuid(SERVICE_UUID);

    let mut characteristics = HashSet::new();
    characteristics.insert(create_temperature().await);


    Service::new(service_uuid, true, characteristics)
}

pub async fn create_gatt_description(description: impl ToString) -> Descriptor {
    let (rx, mut tx) = channel(1);

    let value = Vec::from(description.to_string());
    let value2 = value.clone();

    let runtime = RUNTIME.clone();
    let runtime = runtime.lock().await;

    runtime.spawn(async move {
        while let Some(event) = tx.next().await {
            if let Event::ReadRequest(read_request) = event {
                read_request.response.send(Response::Success(value.clone())).unwrap();
            }
        }
    });

    Descriptor::new(
        Uuid::from_sdp_short_uuid(0x2901 as u16), // TODO: what uuid???
        Properties::new(Some(Read(Secure::Secure(rx))), None),
        Some(value2),
    )
}