use bluster::gatt::event::Event;

pub trait GattEventHandler {
    fn handle_request(&mut self, event: Event);
}

pub trait SensorDataHandler {
    fn handle_sensor_data(&mut self, data: f32);
}
