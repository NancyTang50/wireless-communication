use bluster::gatt::event::Event;
use futures::{channel::mpsc::Receiver, StreamExt};

use crate::gatt::characteristic::GattDescriptionHandler;

use super::{
    description_characteristic_handler::DescriptionCharacteristicHandler, GattEventHandler,
    SensorDataHandler,
};

pub struct CharacteristicHandler<T>
where
    T: GattEventHandler,
{
    characteristic: T,
    description_handler: DescriptionCharacteristicHandler,
    presentation_format_handler: DescriptionCharacteristicHandler,
}

impl<T> CharacteristicHandler<T>
where
    T: GattEventHandler,
{
    pub fn new(
        characteristic: T,
        description_handler: DescriptionCharacteristicHandler,
        presentation_format_handler: DescriptionCharacteristicHandler,
    ) -> Self {
        Self {
            characteristic,
            description_handler,
            presentation_format_handler,
        }
    }
}

impl<T> CharacteristicHandler<T>
where
    T: GattEventHandler,
{
    // NOTE: keep this method, because of other characteristic that don't have additional senders.
    pub async fn handle_requests(&mut self, characteristic_reciever: &mut Receiver<Event>) {
        tokio::select! {
            Some(event) = self.description_handler.recv_request() => self.description_handler.handle_request(event),
            Some(event) = self.presentation_format_handler.recv_request() => self.presentation_format_handler.handle_request(event),
            Some(event) = characteristic_reciever.next() => self.characteristic.handle_request(event),
        }
    }
}

impl<T> CharacteristicHandler<T>
where
    T: GattEventHandler + SensorDataHandler,
{
    pub async fn handle_requests_sensor_data(
        &mut self,
        characteristic_receiver: &mut Receiver<Event>,
        sensor_receiver: &mut Receiver<f32>,
    ) {
        tokio::select! {
            Some(event) = self.description_handler.recv_request() => self.description_handler.handle_request(event),
            Some(event) = self.presentation_format_handler.recv_request() => self.presentation_format_handler.handle_request(event),
            Some(event) = characteristic_receiver.next() => self.characteristic.handle_request(event),
            Some(sensor_data) = sensor_receiver.next() => self.characteristic.handle_addtional_sender(sensor_data),
        }
    }
}
