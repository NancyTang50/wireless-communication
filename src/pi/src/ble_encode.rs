pub trait BleEncode {
    fn to_ble_bytes(self) -> Vec<u8>;
}

impl BleEncode for f32 {
    fn to_ble_bytes(self) -> Vec<u8> {
        let value = self * 100f32;
        let value_as_i16 = value as i16;
        value_as_i16.to_le_bytes().to_vec()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    
    #[test]
    fn test_conversion_of_f32() {
        assert_eq!(20f32.to_ble_bytes(), vec![208, 7]);
    }
}
