use async_trait::async_trait;
use bluster::gatt::event::Event;

pub trait GattEventHandler {
    fn handle_request(&mut self, event: Event);
}

pub trait SensorDataHandler {
    fn handle_sensor_data(&mut self, data: f32);
}

#[async_trait]
pub trait GattDescriptionHandler {
    async fn recv_request(&mut self) -> Option<Event>;
    fn handle_request(&mut self, event: Event);
}
