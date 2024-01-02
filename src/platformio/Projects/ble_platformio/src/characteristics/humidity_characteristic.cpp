#include "humidity_characteristic.h"

void HumidityCharacteristic::set_value(float new_value) {
    if (significant_change(m_last_humidity, new_value, 0.5)) {
        Serial.print(F("Humidity: ")); 
        Serial.print(new_value); 
        Serial.println(F("%"));

        m_characteristic.setValue(new_value * 100);
        m_last_humidity = new_value;     
    } else {
        Serial.println("No significant humidity change");
    }
}