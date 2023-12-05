#include "ble/BLE.h"
#include <cstdint>
#include <stdint.h>

#define TEMPERATURE_CHARACTERISTIC      0x2A6E
#define HUMIDITY_CHARACTERISTIC         0x2A6F

class EnvironmentalService{
    private:
        BLE &ble;
        int16_t temperature;
        uint16_t humidity;

        ReadOnlyGattCharacteristic<int16_t> temperatureCharacteristic;
        ReadOnlyGattCharacteristic<uint16_t> humidityCharacteristic;

    public:
    EnvironmentalService(BLE &_ble, int16_t temperature = 0, uint16_t humidity = 0):
        ble(_ble),
        temperature(temperature),
        humidity(humidity),
        temperatureCharacteristic(TEMPERATURE_CHARACTERISTIC, &temperature, GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_READ | GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY), 
        humidityCharacteristic(HUMIDITY_CHARACTERISTIC, &humidity, GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_READ | GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY)
    {
        GattCharacteristic *charTable[] = {&temperatureCharacteristic, &humidityCharacteristic};
        GattService         environmentalService(GattService::UUID_ENVIRONMENTAL_SERVICE, charTable, sizeof(charTable) / sizeof(GattCharacteristic*));

        ble.addService(environmentalService);
    }

    void updateTemperature(float newTemperature){
        temperature = (int16_t)(newTemperature * 100);
        ble.gattServer().write(temperatureCharacteristic.getValueHandle(), (uint8_t*)&temperature, sizeof(int16_t));
    } 

    void updateHumidity(int16_t newHumidity){
        humidity = (int16_t)(newHumidity * 100);
        ble.gattServer().write(humidityCharacteristic.getValueHandle(), (uint8_t*)&humidity, sizeof(int16_t));
    }
};


// #ifndef CLIMATE_SERVICE_H_
// #define CLIMATE_SERVICE_H_

// #define CLIMATE_SERVICE_UUID      "00000001-710e-4a5b-8d75-3e5b444bc3cf"
// #define HUMIDITY_CHARACTERISTIC "00000002-710e-4a5b-8d75-3e5b444bc3cf"
// #define TEMPERATURE_CHARACTERISTIC    "00000003-710e-4a5b-8d75-3e5b444bc3cf"

// #include "ble/BLE.h"

// class ClimateService {
// public:
//     /**
//      * @param[in] _ble
//      *               BLE object for the underlying controller.
//      * @param[in] level
//      *               8bit batterly level. Usually used to represent percentage of batterly charge remaining.
//      */
//     ClimateService(BLE &_ble, uint8_t temperature = 0, uint8_t humidity = 0) :
//         ble(_ble),
//         humidityCharacteristic(HUMIDITY_CHARACTERISTIC, &temperature, GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY),
//         temperatureCharacteristic(TEMPERATURE_CHARACTERISTIC, &temperature, GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY) {

//         GattCharacteristic *charTable[] = {&temperatureCharacteristic, &humidityCharacteristic};
//         GattService         climateService(CLIMATE_SERVICE_UUID, charTable, sizeof(charTable) / sizeof(GattCharacteristic *));

//         ble.gattServer().addService(climateService);
//     }

//     /**
//      * @brief Update the battery level with a new value. Valid values lie between 0 and 100,
//      * anything outside this range will be ignored.
//      *
//      * @param newLevel
//      *              Update to battery level.
//      */
//     void UpdateTemperature(uint8_t newTemperatureValue) {
//         temperature = newTemperatureValue;
//         ble.gattServer().write(temperatureCharacteristic.getValueHandle(), &temperature, 1);
//     }

//     void UpdateHumidity(uint8_t newHumidityValue) {
//         humidity = newHumidityValue;
//         ble.gattServer().write(humidityCharacteristic.getValueHandle(), &humidity, 1);
//     }

// protected:
//     /**
//      * A reference to the underlying BLE instance that this object is attached to.
//      * The services and characteristics will be registered in this BLE instance.
//      */
//     BLE &ble;

//     /**
//      * The current battery level represented as an integer from 0% to 100%.
//      */
//     uint8_t    temperature;
//     uint8_t    humidity;
//     /**
//      * A ReadOnlyGattCharacteristic that allows access to the peer device to the
//      * batteryLevel value through BLE.
//      */
//     ReadOnlyGattCharacteristic<uint8_t> temperatureCharacteristic;
//     ReadOnlyGattCharacteristic<uint8_t> humidityCharacteristic;
// };
// #endif