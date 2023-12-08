use dht22_pi::{Reading, ReadingError};
use futures::{channel::mpsc::Sender, SinkExt};
use std::time::Duration;
use tracing::{debug, error};

const DHT_22_GPIO_PIN: u8 = 4;
const SENSOR_READ_TIMOUT_IN_MS: u64 = 1000;
const TEMPERATURE_THRESHOLD: f32 = 0.2;
const HUMIDITY_THRESHOLD: f32 = 1.5;

pub fn start_reading_sensor_data(
    mut temperature_changed_channel: Sender<f32>,
    mut humidity_changed_channel: Sender<f32>,
) {
    tokio::spawn(async move {
        let mut old_temperature = f32::MIN;
        let mut old_humidity = f32::MIN;

        loop {
            match dht22_pi::read(DHT_22_GPIO_PIN) {
                Ok(new_reading) => {
                    let (is_temperature_changed, is_humidity_changed) =
                        is_reading_changed(new_reading, old_temperature, old_humidity);

                    if is_temperature_changed {
                        match temperature_changed_channel
                            .send(new_reading.temperature)
                            .await
                        {
                            Ok(_) => {
                                debug!("Temperature changed to {}", new_reading.temperature);
                            }
                            Err(e) => {
                                error!("Error sending temperature update: {:?}", e);
                            }
                        }
                        old_temperature = new_reading.temperature;
                    }

                    if is_humidity_changed {
                        match humidity_changed_channel.send(new_reading.humidity).await {
                            Ok(_) => {
                                debug!("Humidity changed to {}", new_reading.humidity);
                            }
                            Err(e) => {
                                error!("Error sending humidity update: {:?}", e);
                            }
                        }
                        old_humidity = new_reading.humidity;
                    }
                }
                Err(e) => {
                    if let ReadingError::Gpio(e) = e {
                        error!("Error with reading sensor data {:?}", e);
                    }
                }
            }

            std::thread::sleep(Duration::from_millis(SENSOR_READ_TIMOUT_IN_MS));
        }
    });
}

fn is_reading_changed(
    new_reading: Reading,
    old_temperature: f32,
    old_humidity: f32,
) -> (bool, bool) {
    (
        is_not_within_threshold(
            new_reading.temperature,
            old_temperature,
            TEMPERATURE_THRESHOLD,
        ),
        is_not_within_threshold(new_reading.humidity, old_humidity, HUMIDITY_THRESHOLD),
    )
}

fn is_not_within_threshold(a: f32, b: f32, threshold: f32) -> bool {
    (a - b).abs() >= threshold
}

#[cfg(test)]
mod tests {
    use super::is_not_within_threshold;

    #[test]
    fn test_not_within_threshold() {
        assert!(is_not_within_threshold(1.1, 1.2, 0.1));
        assert!(!is_not_within_threshold(1.1, 1.2, 0.2));
        assert!(is_not_within_threshold(1.1, 1.6, 0.5));
        assert!(!is_not_within_threshold(1.1, 1.6, 0.6));
    }
}
