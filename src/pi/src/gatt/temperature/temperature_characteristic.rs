use std::collections::HashSet;

use bluster::{gatt::{characteristic::{Characteristic, Properties, Read, Secure}, descriptor::Descriptor, event::Response}, SdpShortUuid};
use futures::{channel::mpsc::channel, StreamExt};
use tracing::debug;
use uuid::Uuid;

use crate::{TEMPERATURE_CHARACTERISTIC_UUID, gatt::{create_gatt_description, create_gatt_characteristic_presentation_format}};

async fn create_descriptors() -> HashSet<Descriptor> {
    let mut descriptors = HashSet::new();

    descriptors.insert(create_gatt_description("Temperature").await);
    descriptors.insert(create_gatt_characteristic_presentation_format(4,1,44327,0,0).await);

    descriptors
}

pub async fn create_temperature() -> Characteristic {
    let (tx, mut rx) = channel(1);

    tokio::spawn(async move {
        loop {
            if let Some(event) = rx.next().await {
                debug!("Temperature event {:?}", event);
                match event {
                    bluster::gatt::event::Event::ReadRequest(read_request) => {
                        read_request.response.send(Response::Success(vec![20])).unwrap();
                    },
                    bluster::gatt::event::Event::WriteRequest(_) => todo!(),
                    bluster::gatt::event::Event::NotifySubscribe(_) => todo!(),
                    bluster::gatt::event::Event::NotifyUnsubscribe => todo!(),
                }
            }
        }
    });

    Characteristic::new(
        Uuid::from_sdp_short_uuid(TEMPERATURE_CHARACTERISTIC_UUID),
        Properties::new(
            Some(Read(Secure::Insecure(tx.clone()))),
            None,
            None,
            Some(tx),
        ),
        Some(vec![10]),
        create_descriptors().await,
    )

}