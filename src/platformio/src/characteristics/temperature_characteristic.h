#ifndef TEMPERATURE_CHARACTERISTIC_H
#define TEMPERATURE_CHARACTERISTIC_H

#include "characteristic.h"
#include <BLEPeripheral.h>

class TemperatureCharacteristic: public Characteristic<BLEUnsignedShortCharacteristic, float>  {
private:
    float m_last_temperature;
public:
    TemperatureCharacteristic() : Characteristic(BLEUnsignedShortCharacteristic("00002A6E00001000800000805F9B34FB", BLERead | BLENotify))
    {
    }

    ~TemperatureCharacteristic() {}
    
    void set_value(float new_value) override; 
};

#endif
