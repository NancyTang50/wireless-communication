use std::time::SystemTime;

use time::Month;
use chrono::{DateTime, TimeZone, Utc, Datelike, Timelike};
use tracing::debug;

use crate::ble_encode::{BleDecode, BleEncode};

pub struct BleDateTime {
    pub year: u16,
    pub month: Month,
    pub day: u8,
    pub hours: u8,
    pub minutes: u8,
    pub seconds: u8,
}

impl BleDateTime {
    pub fn new(year: u16, month: Month, day: u8, hours: u8, minutes: u8, seconds: u8) -> Self {
        debug_assert!(year >= 1582);
        debug_assert!(year <= 9999);
        debug_assert!(day >= 1);
        debug_assert!(day <= 31);
        debug_assert!(hours <= 23);
        debug_assert!(minutes <= 59);
        debug_assert!(seconds <= 59);

        Self {
            year,
            month,
            day,
            hours,
            minutes,
            seconds,
        }
    }

    pub fn from_system_time() -> Self {
        let system_time = SystemTime::now();
        let current_date_time: DateTime<chrono::Utc> = system_time.into();
        let month = current_date_time.month() as u8;
        let month = Month::try_from(month).unwrap_or(Month::January);
        Self::new(current_date_time.year() as u16, month, current_date_time.day() as u8, current_date_time.hour() as u8, current_date_time.minute() as u8, current_date_time.second() as u8)
    }

    pub fn to_epoch_seconds(&self) -> i64 {
        let month = self.month as u8;
        let date_time = Utc.with_ymd_and_hms(self.year.into(), month.into(), self.day.into(), self.hours.into(), self.minutes.into(), self.seconds.into()).unwrap();
        debug!("To epoch is {:?}", date_time);
        date_time.timestamp()
    }
}

impl BleEncode for BleDateTime {
    fn to_ble_bytes(self) -> Vec<u8> {
        let mut ble_bytes = Vec::new();

        ble_bytes.append(&mut self.year.to_le_bytes().to_vec());
        ble_bytes.push(self.month as u8);
        ble_bytes.push(self.day);
        ble_bytes.push(self.hours);
        ble_bytes.push(self.minutes);
        ble_bytes.push(self.seconds);
        ble_bytes.push(0); // TODO: Unknown day of the week
        ble_bytes.push(0); // TODO: Unknown Fractions265 https://github.com/t2t-sonbui/BLECurrentTimeServiceServer/blob/master/app/src/main/java/xyz/vidieukhien/embedded/ble/currenttimeservice/server/TimeProfile.java
        
        // TODO: Set your adjust reasons accordingly, in my case they were unnecessary
        // Adjust Reasons: Manual Update, External Update, Time Zone Change, Daylight Savings Change
        // const adjustReasons = [true, false, false, true];

        ble_bytes.push(0); // TODO: Unknown update reason

        ble_bytes
    }
}

impl BleDecode for BleDateTime {
    type Output = BleDateTime;

    fn from_ble_byte(ble_bytes: Vec<u8>) -> Self::Output {
        let ble_bytes_len = ble_bytes.len();
        let mut ble_bytes = ble_bytes.into_iter();
        let year = if ble_bytes_len >= 2 {
            u16::from_le_bytes([
                ble_bytes.next().expect("Should not be None"),
                ble_bytes.next().expect("Should not be None"),
            ])
        } else {
            1582u16
        };
        let month = Month::try_from(ble_bytes.next().unwrap_or(1)).unwrap_or(Month::January);
        let day = ble_bytes.next().unwrap_or(1);
        let hours = ble_bytes.next().unwrap_or(0);
        let minutes = ble_bytes.next().unwrap_or(0);
        let seconds = ble_bytes.next().unwrap_or(0);

        Self {
            year,
            month,
            day,
            hours,
            minutes,
            seconds,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use rstest::rstest;

    #[test]
    fn test_conversion_of_ble_date_time() {
        let ble_date_time = BleDateTime::new(2023, Month::December, 8, 12, 10, 11);
        let expected_encoded_datetime: [u8; 7] = [0xE7, 0x07, 0x0C, 0x08, 0x0C, 0x0A, 0x0B];

        assert_eq!(ble_date_time.to_ble_bytes(), expected_encoded_datetime);
    }

    #[rstest]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08, 0x0C, 0x0A, 0x0B], 2023, Month::December, 8, 12, 10, 11)]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08, 0x0C, 0x0A], 2023, Month::December, 8, 12, 10, 0)]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08, 0x0C], 2023, Month::December, 8, 12, 0, 0)]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08], 2023, Month::December, 8, 0, 0, 0)]
    #[case(vec![0xE7, 0x07, 0x0C], 2023, Month::December, 1, 0, 0, 0)]
    #[case(vec![0xE7, 0x07], 2023, Month::January, 1, 0, 0, 0)]
    #[case(vec![], 1582, Month::January, 1, 0, 0, 0)]
    fn test_conversion_of_ble_bytes_to_date_time(
        #[case] ble_bytes: Vec<u8>,
        #[case] expected_year: u16,
        #[case] expected_month: Month,
        #[case] expected_day: u8,
        #[case] expected_hours: u8,
        #[case] expected_minutes: u8,
        #[case] expected_seconds: u8,
    ) {
        let ble_date_time = BleDateTime::from_ble_byte(ble_bytes);

        assert_eq!(ble_date_time.year, expected_year);
        assert_eq!(ble_date_time.month, expected_month);
        assert_eq!(ble_date_time.day, expected_day);
        assert_eq!(ble_date_time.hours, expected_hours);
        assert_eq!(ble_date_time.minutes, expected_minutes);
        assert_eq!(ble_date_time.seconds, expected_seconds);
    }

    #[test]
    fn test_encoding_and_decoding_the_same() {
        let ble_date_time = BleDateTime::new(2023, Month::December, 8, 12, 10, 11);
        let ble_date_time_bytes = ble_date_time.to_ble_bytes();
        let ble_date_time = BleDateTime::from_ble_byte(ble_date_time_bytes);

        assert_eq!(ble_date_time.year, 2023);
        assert_eq!(ble_date_time.month, Month::December);
        assert_eq!(ble_date_time.day, 8);
        assert_eq!(ble_date_time.hours, 12);
        assert_eq!(ble_date_time.minutes, 10);
        assert_eq!(ble_date_time.seconds, 11);
    }
}
