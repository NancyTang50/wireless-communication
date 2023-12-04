use std::collections::HashSet;

use bluster::{
    gatt::{
        characteristic::Characteristic,
        descriptor::{Descriptor, Properties, Read, Secure},
    },
    SdpShortUuid,
};
use futures::channel::mpsc::channel;
use uuid::Uuid;

use super::{
    description_characteristic_handler::DescriptionCharacteristicHandler, CharacteristicHandler,
    GattEventHandler,
};

pub trait CharacteristicCreator<T>
where
    T: GattEventHandler,
{
    fn create_handler_and_descriptors(
        characteristic: T,
        description_text: impl ToString,
        format: u8,
        exponent: u8,
        unit: u16,
        namespace: u8,
        description: u16,
    ) -> (CharacteristicHandler<T>, HashSet<Descriptor>) {
        let (description_char, description_char_handler) =
            create_gatt_description(description_text);
        let (presentation_char, presentation_char_handler) =
            create_gatt_characteristic_presentation_format(
                format,
                exponent,
                unit,
                namespace,
                description,
            );

        let mut descriptors = HashSet::new();
        descriptors.insert(description_char);
        descriptors.insert(presentation_char);

        (
            CharacteristicHandler::new(
                characteristic,
                description_char_handler,
                presentation_char_handler,
            ),
            descriptors,
        )
    }

    fn create_characteristic() -> Characteristic;
}

fn create_gatt_description(
    description: impl ToString,
) -> (Descriptor, DescriptionCharacteristicHandler) {
    let (tx, rx) = channel(1);

    let value = Vec::from(description.to_string());

    (
        Descriptor::new(
            Uuid::from_sdp_short_uuid(0x2901_u16), // org.bluetooth.descriptor.gatt.characteristic_user_description
            Properties::new(Some(Read(Secure::Insecure(tx))), None),
            Some(value.clone()),
        ),
        DescriptionCharacteristicHandler::new(value, rx),
    )
}

fn create_gatt_characteristic_presentation_format(
    format: u8,
    exponent: u8,
    unit: u16,
    namespace: u8,
    description: u16,
) -> (Descriptor, DescriptionCharacteristicHandler) {
    let (tx, rx) = channel(1);
    let [unit_hi, unit_low] = unit.to_le_bytes();
    let [description_hi, description_low] = description.to_le_bytes();

    let value = vec![
        format,
        exponent,
        unit_hi,
        unit_low,
        namespace,
        description_hi,
        description_low,
    ];

    (
        Descriptor::new(
            Uuid::from_sdp_short_uuid(0x2901_u16), // org.bluetooth.descriptor.gatt.characteristic_presentation_format
            Properties::new(Some(Read(Secure::Insecure(tx))), None),
            Some(value.clone()),
        ),
        DescriptionCharacteristicHandler::new(value, rx),
    )
}
