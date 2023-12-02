use std::collections::HashSet;

use bluster::{gatt::{characteristic::{Characteristic, Properties, Read, Secure}, descriptor::Descriptor}, SdpShortUuid};
use futures::{channel::mpsc::channel, StreamExt};
use tracing::debug;
use uuid::Uuid;

use crate::{TEMPERATURE_CHARACTERISTIC_UUID, gatt::create_gatt_description, RUNTIME};

async fn create_descriptors() -> HashSet<Descriptor> {
    let mut descriptors = HashSet::new();

    descriptors.insert(create_gatt_description("Temperature").await);

    // NOTE: somthing about presentation format?

    descriptors
}

pub async fn create_temperature() -> Characteristic {
    debug!("Creating temperature characteristic");
    let (rx, mut tx) = channel(1);

    let runtime = RUNTIME.clone();
    let runtime = runtime.lock().await;

    runtime.spawn(async move {
        while let Some(event) = tx.next().await {
            match event {
                bluster::gatt::event::Event::ReadRequest(_) => todo!(),
                bluster::gatt::event::Event::WriteRequest(_) => todo!(),
                bluster::gatt::event::Event::NotifySubscribe(_) => todo!(),
                bluster::gatt::event::Event::NotifyUnsubscribe => todo!(),
            }
        }
    });
    std::mem::drop(runtime);

    Characteristic::new(
        Uuid::from_sdp_short_uuid(TEMPERATURE_CHARACTERISTIC_UUID),
        Properties::new(
            Some(Read(Secure::Secure(rx.clone()))),
            None,
            Some(rx),
            None,
        ),
        None,
        create_descriptors().await,
    )

}