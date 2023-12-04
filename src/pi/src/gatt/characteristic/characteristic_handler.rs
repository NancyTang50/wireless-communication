use super::{
    description_characteristic_handler::DescriptionCharacteristicHandler, GattEventHandler,
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
    pub async fn handle_requests(&mut self) {
        tokio::select! {
            Some(event) = self.description_handler.recv_request() => self.description_handler.handle_request(event),
            Some(event) = self.presentation_format_handler.recv_request() => self.presentation_format_handler.handle_request(event),
            Some(event) = self.characteristic.recv_request() => self.characteristic.handle_request(event),
        }
    }
}
