#include "temperature_characteristic.h"

void TemperatureCharacteristic::set_value(float new_value) {
    if (significant_change(m_last_temperature, new_value, 0.5)) {
        Serial.print(F("Temperature: ")); 
        Serial.print(new_value); 
        Serial.println(F("C"));

        m_characteristic.setValue(new_value * 100);
        m_last_temperature = new_value;
    } else {
        Serial.println("No significant temperature change");
    }
}
