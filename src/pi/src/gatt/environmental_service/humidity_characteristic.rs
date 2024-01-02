use std::collections::HashSet;

use bluster::{
    gatt::{
        characteristic::{Characteristic, Properties, Read, Secure},
        event::{Event, Response},
    },
    SdpShortUuid,
};
use futures::channel::mpsc::{channel, Receiver, Sender};
use tracing::{debug, info};
use uuid::Uuid;

use crate::{
    ble_encode::BleEncode,
    gatt::characteristic::{GattEventHandler, SensorDataHandler, CharacteristicHandler},
    HUMIDITY_CHARACTERISTIC_UUID,
};

pub struct HumidityCharacteristic {
    humidity: f32,
    notifier: Option<Sender<Vec<u8>>>,
}

impl HumidityCharacteristic {
    fn new() -> Self {
        Self {
            humidity: f32::MIN,
            notifier: None,
        }
    }

    pub fn create_characteristic(humidity_changed_receiver: Receiver<f32>) -> Characteristic {
        let (tx, mut rx) = channel(1);

        let mut handler = CharacteristicHandler::new(Self::new());

        tokio::spawn(async move {
            let mut sensor_rx = humidity_changed_receiver;
            loop {
                handler
                    .handle_requests_sensor_data(&mut rx, &mut sensor_rx)
                    .await;
            }
        });

        Characteristic::new(
            Uuid::from_sdp_short_uuid(HUMIDITY_CHARACTERISTIC_UUID),
            Properties::new(
                Some(Read(Secure::Insecure(tx.clone()))),
                None,
                Some(tx),
                None,
            ),
            Some(vec![]),
            HashSet::new(),
        )
    }
}

impl GattEventHandler for HumidityCharacteristic {
    fn handle_request(&mut self, event: Event) {
        info!("Humidity event {:?}", event);
        match event {
            bluster::gatt::event::Event::ReadRequest(read_request) => {
                read_request
                    .response
                    .send(Response::Success(self.humidity.to_ble_bytes()))
                    .unwrap();
            }
            bluster::gatt::event::Event::WriteRequest(_) => {}
            bluster::gatt::event::Event::NotifySubscribe(notify_event) => {
                self.notifier = Some(notify_event.notification);
            }
            bluster::gatt::event::Event::NotifyUnsubscribe => {
                self.notifier = None;
            }
        }
    }
}

impl SensorDataHandler for HumidityCharacteristic {
    fn handle_sensor_data(&mut self, data: f32) {
        self.humidity = data;
        if let Some(notifier) = &mut self.notifier {
            match notifier.try_send(data.to_ble_bytes()) {
                Ok(_) => {
                    debug!("Send new humidity");
                }
                Err(error) => {
                    // NOTE: if this crashes try also kicking the notifier if the channel is full,
                    // because that means the notifier is not reading the messages
                    if error.is_disconnected() {
                        self.notifier = None;
                    }
                }
            }
        }
    }
}
