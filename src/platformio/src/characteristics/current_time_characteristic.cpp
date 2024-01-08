#include "current_time_characteristic.h"
#include <TimeLib.h>

void CurrentTimeCharacteristic::set_value(time_t new_value) {
    // NOTE: do we need to free the old current_time_value if we set a new one?
    unsigned char current_time_value[CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE];

    auto current_year = year(new_value);
    auto current_month = month(new_value);
    auto current_day = day(new_value);
    auto current_hour = hour(new_value);
    auto current_minutes = minute(new_value);
    auto current_seconds = second(new_value);

    unsigned char year_hi = (current_year >> 8) & 0xFF;
    unsigned char year_lo = current_year & 0xFF;
    current_time_value[0] = year_lo;
    current_time_value[1] = year_hi;
    current_time_value[2] = current_month;
    current_time_value[3] = current_day;
    current_time_value[4] = current_hour;
    current_time_value[5] = current_minutes;
    current_time_value[6] = current_seconds;
    current_time_value[7] = 0; // NOTE: unknown day of the week
    current_time_value[8] = 0; // NOTE: unknown fractions
    current_time_value[9] = 0x08; // NOTE: the update reason is Manual Update 

    m_characteristic.setValue(current_time_value, CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE);
    m_characteristic.value();
}

void CurrentTimeCharacteristic::update_current_time_when_written() {
    if (m_characteristic.written()) {
        change_system_time(m_characteristic.value());
    }
}

void CurrentTimeCharacteristic::change_system_time(const unsigned char* ble_bytes) {
    unsigned char values[CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE];

    memcpy(values, ble_bytes, CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE * sizeof(unsigned char));

    Serial.print("Parsed Date and Time: 0x");
    for (u_int8_t i = 0; i < sizeof(values) / sizeof(values[0]); i++) {
        Serial.print(values[i], HEX);
        Serial.print(" ");
    }
    Serial.println();

    auto year_hi = values[0];
    auto year_lo = values[1];
    uint16_t year = ((uint16_t)year_lo << 8) | year_hi;
    auto month = values[2];
    auto day = values[3];
    auto hours = values[4];
    auto minutes = values[5];
    auto seconds = values[6];
    [[maybe_unused]]auto day_of_week = values[7];
    [[maybe_unused]]auto fractions_265 = values[8];
    [[maybe_unused]]auto adjust_reasons = values[9];

    setTime(hours, minutes, seconds, day, month, year);
}
