#ifndef CURRENT_TIME_CHARACTERISTIC_H
#define CURRENT_TIME_CHARACTERISTIC_H

#include "characteristic.h"
#include <BLEPeripheral.h>

#define CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE  12

class CurrentTimeCharacteristic: public Characteristic<BLECharacteristic, time_t>  {
public:
    CurrentTimeCharacteristic() : 
        Characteristic(BLECharacteristic("00002A2B00001000800000805F9B34FB", BLERead | BLEWrite, CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE))
    {
    }

    ~CurrentTimeCharacteristic() {}
    
    void set_value(time_t new_value) override; 

    void update_current_time_when_written();

    void change_system_time(const unsigned char* ble_bytes);
};

#endif