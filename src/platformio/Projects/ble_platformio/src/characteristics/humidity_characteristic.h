#ifndef HUMIDITY_CHARACTERISTIC_H
#define HUMIDITY_CHARACTERISTIC_H

#include "characteristic.h"
#include <BLEPeripheral.h>

class HumidityCharacteristic: public Characteristic<BLEUnsignedShortCharacteristic, float>  {
private:
    float m_last_humidity;
public:
    HumidityCharacteristic() : Characteristic(BLEUnsignedShortCharacteristic("00002A6F00001000800000805F9B34FB", BLERead | BLENotify))
    {
    }

    ~HumidityCharacteristic() {}
    
    void set_value(float new_value) override; 
};

#endif