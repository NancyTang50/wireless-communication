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
    gatt::characteristic::{GattEventHandler, setup_handler_and_descriptors},
    HUMIDITY_CHARACTERISTIC_UUID,
};

pub struct HumidityCharacteristic {
    rx: Receiver<Event>,
}

impl HumidityCharacteristic {
    fn new(rx: Receiver<Event>) -> Self {
        Self { rx }
    }
    pub fn create_characteristic() -> Characteristic {
        let (rx, tx) = channel(1);

        let (mut handler, descriptors) = setup_handler_and_descriptors!(
            HumidityCharacteristic::new(rx),
            "Humidity",
            4,
            1,
            44327,
            0,
            0
        );

        tokio::spawn(async move {
            loop {
                handler.handle_requests().await;
            }
        });

        Characteristic::new(
            Uuid::from_sdp_short_uuid(HUMIDITY_CHARACTERISTIC_UUID),
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
impl GattEventHandler for HumidityCharacteristic {
    async fn recv_request(&mut self) -> Option<Event> {
        self.rx.next().await
    }

    fn handle_request(&mut self, event: Event) {
        info!("Humidity event {:?}", event);
        match event {
            bluster::gatt::event::Event::ReadRequest(read_request) => {
                read_request
                    .response
                    .send(Response::Success(50.5f32.to_ble_bytes())) // 50.50% humidity
                    .unwrap();
            }
            bluster::gatt::event::Event::WriteRequest(_)
            | bluster::gatt::event::Event::NotifySubscribe(_)
            | bluster::gatt::event::Event::NotifyUnsubscribe => {}
        }
    }
}
