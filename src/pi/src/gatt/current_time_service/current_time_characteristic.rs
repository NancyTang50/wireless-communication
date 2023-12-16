use std::{os::linux, time::SystemTime};

use bluster::{
    gatt::{
        characteristic::{Characteristic, Properties, Read, Secure, Write},
        event::{Event, Response},
    },
    SdpShortUuid,
};
use chrono::DateTime;
use futures::channel::mpsc::channel;
use tracing::{debug, info};
use uuid::Uuid;

use crate::{
    ble_encode::{BleEncode, BleDecode},
    gatt::characteristic::{setup_handler_and_descriptors, GattEventHandler},
    CURRENT_TIME_CHARACTERISTIC_UUID, ble_date_time::BleDateTime,
};

pub struct CurrentTimeCharacteristic;

impl CurrentTimeCharacteristic {
    fn new() -> Self {
        Self {}
    }

    setup_handler_and_descriptors!(Self, "Current Time", 4, 1, 44327, 0, 0);

    pub fn create_characteristic() -> Characteristic {
        let (tx, mut rx) = channel::<Event>(1);

        let (mut handler, descriptors) = Self::create_handler_and_get_descriptors(Self::new());

        tokio::spawn(async move {
            loop {
                handler.handle_requests(&mut rx).await;
            }
        });

        Characteristic::new(
            Uuid::from_sdp_short_uuid(CURRENT_TIME_CHARACTERISTIC_UUID),
            Properties::new(
                Some(Read(Secure::Insecure(tx.clone()))),
                Some(Write::WithResponse(Secure::Insecure(tx))),
                None,
                None,
            ),
            Some(vec![]),
            descriptors,
        )
    }
}

impl GattEventHandler for CurrentTimeCharacteristic {
    fn handle_request(&mut self, event: Event) {
        info!("Time event {:?}", event);
        match event {
            bluster::gatt::event::Event::ReadRequest(read_request) => {
                let bytes = BleDateTime::from_system_time().to_ble_bytes();
                debug!("Sending time as {:?}", bytes);
                read_request.response.send(Response::Success(bytes)).unwrap();
            }
            bluster::gatt::event::Event::WriteRequest(write_request) => {
                debug!("Got time as {:?}", &write_request.data);
                let ble_date_time = BleDateTime::from_ble_byte(write_request.data);
                let tv = linux_api::time::timeval::from_seconds(ble_date_time.to_epoch_seconds());
                unsafe {
                    let tv_ptr: *const _ = &tv;
                    time_sys::settimeofday(tv_ptr, std::ptr::null());
                }

                let system_time = SystemTime::now();
                let current_date_time: DateTime<chrono::Utc> = system_time.into();

                debug!("Current time is {:?}", current_date_time);
                
                // TODO: does a CTS need to send something back on a write request.
                // write_request
                //     .response
                //     .send(Response::Success(vec![0x13])) // A successfull write?
                //     .unwrap();
            }
            bluster::gatt::event::Event::NotifySubscribe(_)
            | bluster::gatt::event::Event::NotifyUnsubscribe => {}
        }
    }
}
