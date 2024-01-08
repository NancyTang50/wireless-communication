// Copyright (c) Sandeep Mistry. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include <Arduino.h>
#include <TimeLib.h>

#include "characteristics/temperature_characteristic.h"
#include "characteristics/humidity_characteristic.h"
#include "characteristics/current_time_characteristic.h"

// Import libraries (BLEPeripheral depends on SPI)
#include <SPI.h>
#include <BLEPeripheral.h>
#include <DHT.h>
#include <Adafruit_Sensor.h>
#include <arduino-timer.h>

// DHT22 sensor pin
#define DHT22_PIN A5
#define CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE  12

Timer<> timer = timer_create_default();

volatile bool read_from_sensor = true;
volatile bool update_time = false;

// custom boards may override default pin definitions with BLEPeripheral(PIN_REQ, PIN_RDY, PIN_RST)
BLEPeripheral ble_peripheral = BLEPeripheral();

// The environmental sense service
BLEService environmental_service = BLEService("0000181A00001000800000805F9B34FB");
TemperatureCharacteristic temperature_characteristic = TemperatureCharacteristic();
HumidityCharacteristic humidity_characteristic = HumidityCharacteristic();

// The time service
BLEService current_time_service = BLEService("0000180500001000800000805F9B34FB");
CurrentTimeCharacteristic current_time_characteristic = CurrentTimeCharacteristic();

DHT dht(DHT22_PIN, DHT22); 

void ble_peripheral_connect_handler(BLECentral& central) {
    Serial.print("Connected event, central: ");
    Serial.println(central.address());
}

void ble_peripheral_disconnect_handler(BLECentral& central) {
    Serial.print("Disconnected event, central: ");
    Serial.println(central.address());
}

bool update_read_sensor(void *) {
    read_from_sensor = true;
    return true; // NOTE: this is to repeat the timer
}

bool update_time_value(void *) {
    update_time = true;
    return true; // NOTE: this is to repeat the timer
}

void setup()
{
    Serial.begin(9600);
#if defined(__AVR_ATmega32U4__)
    delay(5000); // 5 seconds delay for enabling to see the start up comments on the serial board
#endif

    dht.begin();

    ble_peripheral.setLocalName("RoomSensor-NORDIC");
    ble_peripheral.setAdvertisedServiceUuid(environmental_service.uuid());
    ble_peripheral.setAdvertisedServiceUuid(current_time_service.uuid());

    // add service and characteristic
    ble_peripheral.addAttribute(environmental_service);

    ble_peripheral.addAttribute(temperature_characteristic.get_characteristic());
    ble_peripheral.addAttribute(humidity_characteristic.get_characteristic());

    ble_peripheral.addAttribute(current_time_service);
    ble_peripheral.addAttribute(current_time_characteristic.get_characteristic());

    ble_peripheral.setEventHandler(BLEConnected, ble_peripheral_connect_handler);
    ble_peripheral.setEventHandler(BLEDisconnected, ble_peripheral_disconnect_handler);

    // begin initialization
    ble_peripheral.begin();

    timer.every(60000, update_read_sensor);
    timer.every(500, update_time_value);

    Serial.println(F("BLE LED Peripheral"));
}

void loop()
{
    ble_peripheral.poll();
    
    if(read_from_sensor) {
        auto temperature_reading = dht.readTemperature();
        auto humidity_reading = dht.readHumidity();
        Serial.println("Read the dht");
        
        temperature_characteristic.set_value(temperature_reading);
        humidity_characteristic.set_value(humidity_reading);

        read_from_sensor = false;
    }

    current_time_characteristic.update_current_time_when_written();

    if(update_time) {
        current_time_characteristic.set_value(now());
        update_time = false;
    }

    timer.tick();
}