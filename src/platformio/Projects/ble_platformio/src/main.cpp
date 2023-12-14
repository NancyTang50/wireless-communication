// Copyright (c) Sandeep Mistry. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include <Arduino.h>
// Import libraries (BLEPeripheral depends on SPI)
#include <SPI.h>
#include <BLEPeripheral.h>
#include <DHT.h>
#include <Adafruit_Sensor.h>
#include <arduino-timer.h>

// DHT22 sensor pin
#define DHT22_PIN A5

Timer<> timer = timer_create_default();

volatile bool readFromSensor = false;

// NRF52_ISR_Timer SensorReadTimer;

// custom boards may override default pin definitions with BLEPeripheral(PIN_REQ, PIN_RDY, PIN_RST)
BLEPeripheral blePeripheral = BLEPeripheral();

// The environmental sense service
BLEService environmentalService = BLEService("0000181A00001000800000805F9B34FB");

// The temperature characteristic
BLEUnsignedShortCharacteristic temperatureCharacteristic = BLEUnsignedShortCharacteristic("00002A6E00001000800000805F9B34FB", BLERead | BLENotify);
// FIXME: Add descriptor, or should we remove those?

// The humidity characteristic
BLEUnsignedShortCharacteristic humidityCharacteristic = BLEUnsignedShortCharacteristic("00002A6F00001000800000805F9B34FB", BLERead | BLENotify);
// FIXME: Add descriptor, or should we remove those?

DHT dht(DHT22_PIN, DHT22); 
float lastHumidity, lastTemperature;

boolean significantChange(float val1, float val2, float threshold) {
    return (abs(val1 - val2) >= threshold);
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

void setTempCharacteristicValue(float reading) {
    if (!isnan(reading) && significantChange(lastTemperature, reading, 0.5)) {
        Serial.print(F("Temperature: ")); Serial.print(reading); Serial.println(F("C"));

        temperatureCharacteristic.setValue(reading * 100);
        lastTemperature = reading;
    }
}

void setHumidityCharacteristicValue(float reading) {
    if (!isnan(reading) && significantChange(lastHumidity, reading, 1.0)) {
        Serial.print(F("Humidity: ")); Serial.print(reading); Serial.println(F("%"));

        humidityCharacteristic.setValue(reading * 100);
        lastHumidity = reading;
    }
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

    // add service and characteristic
    blePeripheral.addAttribute(environmentalService);
    blePeripheral.addAttribute(temperatureCharacteristic);
    blePeripheral.addAttribute(humidityCharacteristic);

    blePeripheral.setEventHandler(BLEConnected, blePeripheralConnectHandler);
    blePeripheral.setEventHandler(BLEDisconnected, blePeripheralDisconnectHandler);

    // begin initialization
    blePeripheral.begin();

    timer.every(2000, updateReadSensor);

    Serial.println(F("BLE LED Peripheral"));
}

void loop()
{
    blePeripheral.poll();

    if(readFromSensor) {
        Serial.println(F("3"));
        auto temperature_reading = dht.readTemperature();
        Serial.println(F("4"));
        auto humidity_reading = dht.readHumidity();
        Serial.println(F("5"));

        if(!isnan(temperature_reading) && !isnan(humidity_reading)) {
            setTempCharacteristicValue(temperature_reading);
            setHumidityCharacteristicValue(humidity_reading);
        } else {
            Serial.println(F("Failed to read from DHT sensor!"));
        }

        readFromSensor = false;
    }

    timer.tick();
}
