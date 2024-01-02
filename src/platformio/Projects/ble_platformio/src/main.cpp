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

volatile bool readFromSensor = true;
volatile bool updateTime = false;

// NRF52_ISR_Timer SensorReadTimer;

// custom boards may override default pin definitions with BLEPeripheral(PIN_REQ, PIN_RDY, PIN_RST)
BLEPeripheral blePeripheral = BLEPeripheral();

// The environmental sense service
BLEService environmentalService = BLEService("0000181A00001000800000805F9B34FB");

TemperatureCharacteristic temperatureCharacteristic = TemperatureCharacteristic();
HumidityCharacteristic humidityCharacteristic = HumidityCharacteristic();
// The time service
BLEService currentTimeService = BLEService("0000180500001000800000805F9B34FB");
CurrentTimeCharacteristic currentTimeCharacteristic = CurrentTimeCharacteristic();

// The time characteristic
// unsigned char valueSize = CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE;
// BLECharacteristic currentTimeCharacteristic = BLECharacteristic("00002A2B00001000800000805F9B34FB", BLERead | BLEWrite, valueSize);

DHT dht(DHT22_PIN, DHT22); 

void printDigits(int digits) {
  // Add a leading zero if the value is less than 10
  if (digits < 10)
    Serial.print('0');
  Serial.print(digits);
}

void blePeripheralConnectHandler(BLECentral& central) {
    Serial.print(F("Connected event, central: "));
    Serial.println(central.address());
}

void blePeripheralDisconnectHandler(BLECentral& central) {
    Serial.print(F("Disconnected event, central: "));
    Serial.println(central.address());
}

bool updateReadSensor(void *) {
    readFromSensor = true;
    return true; // NOTE: this is to repeat the timer
}

bool updateTimeValue(void *) {
    updateTime = true;
    return true; // NOTE: this is to repeat the timer
}

void setup()
{
    Serial.begin(9600);
#if defined(__AVR_ATmega32U4__)
    delay(5000); // 5 seconds delay for enabling to see the start up comments on the serial board
#endif

    dht.begin();

    blePeripheral.setLocalName("SOME_NAME_NORDIC");
    blePeripheral.setAdvertisedServiceUuid(environmentalService.uuid());
    blePeripheral.setAdvertisedServiceUuid(currentTimeService.uuid());

    // add service and characteristic
    blePeripheral.addAttribute(environmentalService);

    blePeripheral.addAttribute(temperatureCharacteristic.get_characteristic());
    blePeripheral.addAttribute(humidityCharacteristic.get_characteristic());

    blePeripheral.addAttribute(currentTimeService);
    blePeripheral.addAttribute(currentTimeCharacteristic.get_characteristic());

    blePeripheral.setEventHandler(BLEConnected, blePeripheralConnectHandler);
    blePeripheral.setEventHandler(BLEDisconnected, blePeripheralDisconnectHandler);

    // begin initialization
    blePeripheral.begin();

    timer.every(5000, updateReadSensor);
    timer.every(500, updateTimeValue);

    Serial.println(F("BLE LED Peripheral"));
}

void loop()
{
    blePeripheral.poll();
    
    if(readFromSensor) {
        auto temperature_reading = dht.readTemperature();
        auto humidity_reading = dht.readHumidity();
        Serial.println("Read the dht");
        
        temperatureCharacteristic.set_value(temperature_reading);
        humidityCharacteristic.set_value(humidity_reading);

        readFromSensor = false;
    }

    currentTimeCharacteristic.update_current_time_when_written();

    if(updateTime) {
        currentTimeCharacteristic.set_value(now());
        updateTime = false;
    }

    timer.tick();
}