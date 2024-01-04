#include "temperature_characteristic.h"

void TemperatureCharacteristic::set_value(float new_value) {
    if (significant_change(m_last_temperature, new_value, 0.5)) {
        Serial.print("Temperature: "); 
        Serial.print(new_value); 
        Serial.println("C");

        m_characteristic.setValue(new_value * 100);
        m_last_temperature = new_value;
    } else {
        Serial.println("No significant temperature change");
    }
}
