use async_trait::async_trait;
use bluster::gatt::event::{Event, Response};
use futures::{channel::mpsc::Receiver, StreamExt};
use tracing::debug;

use super::GattEventHandler;

pub struct DescriptionCharacteristicHandler {
    value: Vec<u8>,
    rx: Receiver<Event>,
}

impl DescriptionCharacteristicHandler {
    pub fn new(value: Vec<u8>, rx: Receiver<Event>) -> Self {
        Self { value, rx }
    }
}

#[async_trait]
impl GattEventHandler for DescriptionCharacteristicHandler {
    async fn recv_request(&mut self) -> Option<Event> {
        self.rx.next().await
    }

    fn handle_request(&mut self, event: Event) {
        debug!("Event: {:?}", event);
        if let Event::ReadRequest(read_request) = event {
            read_request
                .response
                .send(Response::Success(self.value.clone()))
                .unwrap();
        }
    }
}
