use std::time::Duration;

use dht22_pi::Reading;
use futures::channel::oneshot::{Sender, Receiver, channel};
use tracing::error;
use lazy_static::lazy_static;

const DHT_22_GPIO_PIN: u8 = 4;
const SENSOR_READ_TIMOUT_IN_MS: u64 = 500;

lazy_static! {
    static ref TEMPERATURE_CHANNEL : (Sender<f32>, Receiver<f32>) = channel::<f32>();
    
    static ref HUMIDITY_CHANNEL : (Sender<f32>, Receiver<f32>) = channel::<f32>();
}

pub fn start_reading_sensor_data(temperature_changed_channel: Sender<f32>, humidity_changed_channel: Sender<f32>)
{
    tokio::spawn(async move {
        let mut old_reading = Reading {
            temperature: 0f32,
            humidity: 0f32,
        };

        loop {
            match dht22_pi::read(DHT_22_GPIO_PIN) {
                Ok(new_reading) => {
                    let (is_temperature_changed, is_humidity_changed) = is_reading_changed(new_reading, old_reading);

                    if is_temperature_changed {
                        temperature_changed_channel.send(new_reading.temperature);
                    }

                    if is_humidity_changed {
                        humidity_changed_channel.send(new_reading.humidity);
                    }

                    old_reading = new_reading;
                },
                Err(e) => {
                    error!("Error with reading sensor data {:?}", e);
                }
            }

            std::thread::sleep(Duration::from_millis(SENSOR_READ_TIMOUT_IN_MS));
        }
    });
}

fn is_reading_changed(new_reading: Reading, old_reading: Reading) -> (bool, bool) {
    // TODO: maybe add a threshold?
    (
        new_reading.temperature != old_reading.temperature,
        new_reading.humidity != old_reading.humidity
    )
}