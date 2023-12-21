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
    gatt::characteristic::{setup_handler_and_descriptors, GattEventHandler, SensorDataHandler},
    TEMPERATURE_CHARACTERISTIC_UUID,
};

pub struct TemperatureCharacteristic {
    temperature: f32,
    notifier: Option<Sender<Vec<u8>>>,
}

struct CurrentTime
{
    uint16_t year;
    uint8_t month;
    uint8_t day;
    uint8_t hours;
    uint8_t minutes;
    uint8_t seconds;
};


impl TemperatureCharacteristic {
    fn new() -> Self {
        Self {
            temperature: f32::MIN,
            notifier: None,
        }
    }

    setup_handler_and_descriptors!(Self, "Temperature", 4, 1, 44327, 0, 0);

    pub fn create_characteristic(temperature_changed_receiver: Receiver<f32>) -> Characteristic {
        let (tx, mut rx) = channel::<Event>(1);

        let (mut handler, descriptors) =
            Self::create_handler_and_get_descriptors(TemperatureCharacteristic::new());

        tokio::spawn(async move {
            let mut sensor_rx = temperature_changed_receiver;
            loop {
                handler
                    .handle_requests_sensor_data(&mut rx, &mut sensor_rx)
                    .await;
            }
        });

        Characteristic::new(
            Uuid::from_sdp_short_uuid(TEMPERATURE_CHARACTERISTIC_UUID),
            Properties::new(
                Some(Read(Secure::Insecure(tx.clone()))),
                None,
                Some(tx),
                None,
            ),
            Some(vec![10]),
            descriptors,
        )
    }
}

impl GattEventHandler for TemperatureCharacteristic {
    fn handle_request(&mut self, event: Event) {
        info!("Temperature event {:?}", event);
        match event {
            bluster::gatt::event::Event::ReadRequest(read_request) => {
                read_request
                    .response
                    .send(Response::Success(self.temperature.to_ble_bytes()))
                    .unwrap();
            }
            bluster::gatt::event::Event::WriteRequest(_) => {}
            bluster::gatt::event::Event::NotifySubscribe(notify_request) => {
                self.notifier = Some(notify_request.notification)
            }
            bluster::gatt::event::Event::NotifyUnsubscribe => {
                self.notifier = None;
            }
        }
    }
}

impl SensorDataHandler for TemperatureCharacteristic {
    fn handle_sensor_data(&mut self, data: f32) {
        self.temperature = data;
        if let Some(notifier) = &mut self.notifier {
            match notifier.try_send(data.to_ble_bytes()) {
                Ok(_) => {
                    debug!("Send new temperature");
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
