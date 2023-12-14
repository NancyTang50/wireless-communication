use bluster::{
    gatt::descriptor::{Descriptor, Properties, Read, Secure},
    SdpShortUuid,
};
use futures::channel::mpsc::channel;
use uuid::Uuid;

use super::description_characteristic_handler::DescriptionCharacteristicHandler;

macro_rules! setup_handler_and_descriptors {
    ($handler_type: tt, $description_text: expr, $format: expr, $exponent: expr, $unit: expr, $namespace: expr, $description: expr) => {
        fn create_handler_and_get_descriptors(
            event_handler: $handler_type,
        ) -> (
            $crate::gatt::characteristic::CharacteristicHandler<$handler_type>,
            std::collections::HashSet<bluster::gatt::descriptor::Descriptor>,
        ) {
            let (description_char, description_char_handler) =
                $crate::gatt::characteristic::create_gatt_description($description_text);

            let (presentation_char, presentation_char_handler) =
                $crate::gatt::characteristic::create_gatt_characteristic_presentation_format(
                    $format,
                    $exponent,
                    $unit,
                    $namespace,
                    $description,
                );

            let mut descriptors = std::collections::HashSet::new();
            descriptors.insert(description_char);
            descriptors.insert(presentation_char);

            let handler = $crate::gatt::characteristic::CharacteristicHandler::<$handler_type>::new(
                event_handler,
                description_char_handler,
                presentation_char_handler,
            );

            (handler, descriptors)
        }
    };
}

pub(crate) use setup_handler_and_descriptors;

pub fn create_gatt_description(
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

pub fn create_gatt_characteristic_presentation_format(
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
