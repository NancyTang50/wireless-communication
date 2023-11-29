use btleplug::api::bleuuid::uuid_from_u16;
use uuid::Uuid;

const TEMPERATURE_CHARACTERISTIC_UUID : Uuid = uuid_from_u16(0xFFE9);

fn main() {
    println!("Hello, world!");
}
