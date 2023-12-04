use async_trait::async_trait;
use bluster::{
    gatt::{
        characteristic::{Characteristic, Properties, Read, Secure},
        event::{Event, Response},
    },
    SdpShortUuid,
};
use futures::{
    channel::mpsc::{channel, Receiver},
    StreamExt,
};
use tracing::info;
use uuid::Uuid;

use crate::{
    ble_encode::BleEncode,
    gatt::characteristic::{CharacteristicCreator, GattEventHandler},
    TEMPERATURE_CHARACTERISTIC_UUID,
};

pub struct TemperatureCharacteristic {
    rx: Receiver<Event>,
}

impl TemperatureCharacteristic {
    fn new(rx: Receiver<Event>) -> Self {
        Self { rx }
    }
}

impl CharacteristicCreator<TemperatureCharacteristic> for TemperatureCharacteristic {
    fn create_characteristic() -> Characteristic {
        let (tx, rx) = channel(1);

        let (mut handler, descriptors) = Self::create_handler_and_descriptors(
            TemperatureCharacteristic::new(rx),
            "Temperature",
            4,
            1,
            44327,
            0,
            0,
        );

        tokio::spawn(async move {
            loop {
                handler.handle_requests().await;
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
            descriptors,
        )
    }
}

#[async_trait]
impl GattEventHandler for TemperatureCharacteristic {
    async fn recv_request(&mut self) -> Option<Event> {
        self.rx.next().await
    }

    fn handle_request(&mut self, event: Event) {
        info!("Temperature event {:?}", event);
        match event {
            bluster::gatt::event::Event::ReadRequest(read_request) => {
                read_request
                    .response
                    .send(Response::Success(23.5f32.to_ble_bytes())) // 23.50 degrees
                    .unwrap();
            }
            bluster::gatt::event::Event::WriteRequest(_)
            | bluster::gatt::event::Event::NotifySubscribe(_)
            | bluster::gatt::event::Event::NotifyUnsubscribe => {}
        }
    }
}
