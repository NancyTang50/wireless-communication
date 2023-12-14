// Copyright (c) Sandeep Mistry. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include <Arduino.h>
// Import libraries (BLEPeripheral depends on SPI)
#include <SPI.h>
#include <BLEPeripheral.h>
#include <DHT.h>
#include <Adafruit_Sensor.h>

// LED pin
#define DHT22_PIN A5

// custom boards may override default pin definitions with BLEPeripheral(PIN_REQ, PIN_RDY, PIN_RST)
BLEPeripheral blePeripheral = BLEPeripheral();

// The environmental sense service
BLEService environmentalService = BLEService("0000181A00001000800000805F9B34FB");

// The temperature characteristic
BLECharCharacteristic temperatureCharacteristic = BLECharCharacteristic("00002A6E00001000800000805F9B34FB", BLERead | BLENotify);
// The humidity characteristic
BLECharCharacteristic humidityCharacteristic = BLECharCharacteristic("00002A6F00001000800000805F9B34FB", BLERead | BLENotify);

DHT dht(DHT22_PIN, DHT22); 
float humidity, temperature;

// void loop() {
//   humidity = dht.readHumidity();
//   temperature = dht.readTemperature();

//   Serial.print("Temperature: ");
//   Serial.print(temperature);
//   Serial.print("°C / Humidity: ");
//   Serial.print(humidity);
//   Serial.println("%");

//   delay(5000);

// }

void setup()
{
  Serial.begin(9600);
  dht.begin();
#if defined(__AVR_ATmega32U4__)
  delay(5000); // 5 seconds delay for enabling to see the start up comments on the serial board
#endif

  // set LED pin to output mode
  // pinMode(LED_PIN, OUTPUT);

  // set advertised local name and service UUID
  blePeripheral.setLocalName("LED");
  blePeripheral.setAdvertisedServiceUuid(environmentalService.uuid());

  // add service and characteristic
  blePeripheral.addAttribute(environmentalService);
  blePeripheral.addAttribute(temperatureCharacteristic);

  // begin initialization
  blePeripheral.begin();

  Serial.println(F("BLE LED Peripheral"));
}

void loop()
{
  BLECentral central = blePeripheral.central();

  if (central)
  {
    // central connected to peripheral
    Serial.print(F("Connected to central: "));
    Serial.println(central.address());

    while (central.connected())
    {
      // central still connected to peripheral
      if (temperatureCharacteristic.written())
      {
        // central wrote new value to characteristic, update LED
        if (temperatureCharacteristic.value())
        {
          Serial.println(F("LED on"));
          // digitalWrite(LED_PIN, LOW);
        }
        else
        {
          Serial.println(F("LED off"));
          // digitalWrite(LED_PIN, HIGH);
        }
      }
    }

    // central disconnected
    Serial.print(F("Disconnected from central: "));
    Serial.println(central.address());
  }
}