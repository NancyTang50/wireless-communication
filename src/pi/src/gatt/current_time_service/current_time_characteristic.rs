use std::os::linux;

use bluster::{
    gatt::{
        characteristic::{Characteristic, Properties, Read, Secure},
        event::{Event, Response},
    },
    SdpShortUuid,
};
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
                None,
                None,
                Some(tx),
            ),
            Some(vec![10]),
            descriptors,
        )
    }
}

impl GattEventHandler for CurrentTimeCharacteristic {
    fn handle_request(&mut self, event: Event) {
        info!("Time event {:?}", event);
        match event {
            bluster::gatt::event::Event::ReadRequest(read_request) => {
                
            }
            bluster::gatt::event::Event::WriteRequest(write_request) => {
                let ble_date_time = BleDateTime::from_ble_byte(write_request.data);
                let tv = linux_api::time::timeval::from_seconds(ble_date_time.to_epoch_seconds());
                unsafe {
                    let tv_ptr: *const _ = &tv;
                    time_sys::settimeofday(tv_ptr, std::ptr::null());
                }
                
                write_request
                    .response
                    .send(Response::Success(vec![]))
                    .unwrap();
            }
            bluster::gatt::event::Event::NotifySubscribe(_)
            | bluster::gatt::event::Event::NotifyUnsubscribe => {}
        }
    }
}
