use bluster::gatt::event::Event;
use futures::{channel::mpsc::Receiver, StreamExt};

use super::{GattEventHandler, SensorDataHandler};

pub struct CharacteristicHandler<T>
where
    T: GattEventHandler,
{
    characteristic: T,
}

impl<T> CharacteristicHandler<T>
where
    T: GattEventHandler,
{
    pub fn new(characteristic: T) -> Self {
        Self { characteristic }
    }
}

impl<T> CharacteristicHandler<T>
where
    T: GattEventHandler,
{
    // NOTE: keep this method, because of other characteristic that don't have additional senders.
    pub async fn handle_requests(&mut self, characteristic_reciever: &mut Receiver<Event>) {
        tokio::select! {
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
            Some(event) = characteristic_receiver.next() => self.characteristic.handle_request(event),
            Some(sensor_data) = sensor_receiver.next() => self.characteristic.handle_sensor_data(sensor_data),
        }
    }
}
