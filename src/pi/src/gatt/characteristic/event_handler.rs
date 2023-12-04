use async_trait::async_trait;
use bluster::gatt::event::Event;

#[async_trait]
pub trait GattEventHandler {
    async fn recv_request(&mut self) -> Option<Event>;

    fn handle_request(&mut self, event: Event);
}
