use std::time::SystemTime;

use time::Month;
use chrono::{DateTime, TimeZone, Utc, Datelike, Timelike};
use tracing::debug;
use crate::ble_encode::{BleDecode, BleEncode};

pub struct BleDateTime {
    year: u16,
    month: Month,
    day: u8,
    hours: u8,
    minutes: u8,
    seconds: u8,
    day_of_week: Weekday,
    fractions256: u8,
}

impl BleDateTime {
    
    #[warn(clippy::too_many_arguments)]    
    fn new(year: u16, month: Month, day: u8, hours: u8, minutes: u8, seconds: u8, day_of_week: Weekday, fractions256: u8) -> Self {
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
            day_of_week,
            fractions256,
        }
    }

    pub fn from_system_time() -> Self {
        let system_time = SystemTime::now();
        let current_date_time: DateTime<chrono::Utc> = system_time.into();
        let month = current_date_time.month() as u8;
        let month = Month::try_from(month).unwrap_or(Month::January);
        let day_of_week = Weekday::from(current_date_time.weekday());
        
        Self::new(
            current_date_time.year() as u16, 
            month, 
            current_date_time.day() as u8, 
            current_date_time.hour() as u8, 
            current_date_time.minute() as u8, 
            current_date_time.second() as u8,
            day_of_week,
            (current_date_time.timestamp_subsec_millis() / 256) as u8,
        )
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
        ble_bytes.push(self.day_of_week as u8);
        ble_bytes.push(self.fractions256);
        ble_bytes.push(0x08); // NOTE: the update reason is Manual Update

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
        let day_of_week = Weekday::try_from(ble_bytes.next().unwrap_or(0)).unwrap_or(Weekday::Unknown);
        let fractions256 = ble_bytes.next().unwrap_or(0);

        Self {
            year,
            month,
            day,
            hours,
            minutes,
            seconds,
            day_of_week,
            fractions256,
        }
    }
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Weekday {
    Unknown = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7,
}

impl TryFrom<u8> for Weekday {
    type Error = ();

    fn try_from(value: u8) -> Result<Self, Self::Error> {
        if value > 7 {
            return Err(());
        }

        Ok(match value {
            0 => Weekday::Unknown,
            1 => Weekday::Monday,
            2 => Weekday::Tuesday,
            3 => Weekday::Wednesday,
            4 => Weekday::Thursday,
            5 => Weekday::Friday,
            6 => Weekday::Saturday,
            7 => Weekday::Sunday,
            _ => unreachable!()
        })
    }
}

impl From<chrono::Weekday> for Weekday {
    fn from(value: chrono::Weekday) -> Self {
        match value {
            chrono::Weekday::Mon => Weekday::Monday,
            chrono::Weekday::Tue => Weekday::Tuesday,
            chrono::Weekday::Wed => Weekday::Wednesday,
            chrono::Weekday::Thu => Weekday::Thursday,
            chrono::Weekday::Fri => Weekday::Friday,
            chrono::Weekday::Sat => Weekday::Saturday,
            chrono::Weekday::Sun => Weekday::Sunday,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use rstest::rstest;

    #[test]
    fn test_conversion_of_ble_date_time() {
        let ble_date_time = BleDateTime::new(2023, Month::December, 8, 12, 10, 11, Weekday::Unknown, 0);
        let expected_encoded_datetime: [u8; 10] = [0xE7, 0x07, 0x0C, 0x08, 0x0C, 0x0A, 0x0B, 0x00, 0x00, 0x08];

        assert_eq!(ble_date_time.to_ble_bytes(), expected_encoded_datetime);
    }

    #[rstest]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08, 0x0C, 0x0A, 0x0B, 0x02], 2023, Month::December, 8, 12, 10, 11, Weekday::Tuesday)]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08, 0x0C, 0x0A, 0x0B], 2023, Month::December, 8, 12, 10, 11, Weekday::Unknown)]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08, 0x0C, 0x0A], 2023, Month::December, 8, 12, 10, 0, Weekday::Unknown)]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08, 0x0C], 2023, Month::December, 8, 12, 0, 0, Weekday::Unknown)]
    #[case(vec![0xE7, 0x07, 0x0C, 0x08], 2023, Month::December, 8, 0, 0, 0, Weekday::Unknown)]
    #[case(vec![0xE7, 0x07, 0x0C], 2023, Month::December, 1, 0, 0, 0, Weekday::Unknown)]
    #[case(vec![0xE7, 0x07], 2023, Month::January, 1, 0, 0, 0, Weekday::Unknown)]
    #[case(vec![], 1582, Month::January, 1, 0, 0, 0, Weekday::Unknown)]
    fn test_conversion_of_ble_bytes_to_date_time(
        #[case] ble_bytes: Vec<u8>,
        #[case] expected_year: u16,
        #[case] expected_month: Month,
        #[case] expected_day: u8,
        #[case] expected_hours: u8,
        #[case] expected_minutes: u8,
        #[case] expected_seconds: u8,
        #[case] expected_week_day: Weekday,
    ) {
        let ble_date_time = BleDateTime::from_ble_byte(ble_bytes);

        assert_eq!(ble_date_time.year, expected_year);
        assert_eq!(ble_date_time.month, expected_month);
        assert_eq!(ble_date_time.day, expected_day);
        assert_eq!(ble_date_time.hours, expected_hours);
        assert_eq!(ble_date_time.minutes, expected_minutes);
        assert_eq!(ble_date_time.seconds, expected_seconds);
        assert_eq!(ble_date_time.day_of_week, expected_week_day);
    }

    #[test]
    fn test_encoding_and_decoding_the_same() {
        let ble_date_time = BleDateTime::new(2023, Month::December, 8, 12, 10, 11, Weekday::Unknown, 0);
        let ble_date_time_bytes = ble_date_time.to_ble_bytes();
        let ble_date_time = BleDateTime::from_ble_byte(ble_date_time_bytes);

        assert_eq!(ble_date_time.year, 2023);
        assert_eq!(ble_date_time.month, Month::December);
        assert_eq!(ble_date_time.day, 8);
        assert_eq!(ble_date_time.hours, 12);
        assert_eq!(ble_date_time.minutes, 10);
        assert_eq!(ble_date_time.seconds, 11);
        assert_eq!(ble_date_time.day_of_week, Weekday::Unknown);
    }
}

