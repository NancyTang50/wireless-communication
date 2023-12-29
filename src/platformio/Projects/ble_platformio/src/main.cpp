// Copyright (c) Sandeep Mistry. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include <Arduino.h>
#include <TimeLib.h>


// Import libraries (BLEPeripheral depends on SPI)
#include <SPI.h>
#include <BLEPeripheral.h>
// #include <DHT.h>
#include <Adafruit_Sensor.h>
#include <arduino-timer.h>

// DHT22 sensor pin
#define DHT22_PIN A5
#define CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE  12

Timer<> timer = timer_create_default();

volatile bool readFromSensor = false;
volatile bool updateTime = false;

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

// The time service
BLEService currentTimeService = BLEService("0000180500001000800000805F9B34FB");

// The time characteristic
unsigned char valueSize = CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE;
BLECharacteristic currentTimeCharacteristic = BLECharacteristic("00002A2B00001000800000805F9B34FB", BLERead | BLEWrite, valueSize);

DHT dht(DHT22_PIN, DHT22); 
float lastHumidity, lastTemperature;

void printDigits(int digits) {
  // Add a leading zero if the value is less than 10
  if (digits < 10)
    Serial.print('0');
  Serial.print(digits);
}

void initializeTime(){
    delay(1000);
}

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

bool updateTimeValue(void *) {
    updateTime = true;
    return true; // NOTE: this is to repeat the timer
}

void setTempCharacteristicValue(float reading) {
    if (significantChange(lastTemperature, reading, 0.5)) {
        Serial.print(F("Temperature: ")); 
        Serial.print(reading); Serial.println(F("C"));

        temperatureCharacteristic.setValue(reading * 100);
        lastTemperature = reading;
    }
}

void setHumidityCharacteristicValue(float reading) {
    if (significantChange(lastHumidity, reading, 1.0)) {
        Serial.print(F("Humidity: ")); Serial.print(reading); Serial.println(F("%"));

        humidityCharacteristic.setValue(reading * 100);
        lastHumidity = reading;     
    }
}

void setTimeCharacteristicValue() {
    // NOTE: do we need to free the old current_time_value if we set a new one?
    unsigned char current_time_value[CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE];

    time_t t = now(); // store the current time in time variable t
    auto current_year = year(t);
    auto current_month = month(t);
    auto current_day = day(t);
    auto current_hour = hour(t);
    auto current_minutes = minute(t);
    auto current_seconds = second(t);

    unsigned char year_hi = (current_year >> 8) & 0xFF;
    unsigned char year_lo = current_year & 0xFF;
    current_time_value[0] = year_lo;
    current_time_value[1] = year_hi;
    current_time_value[2] = current_month;
    current_time_value[3] = current_day;
    current_time_value[4] = current_hour;
    current_time_value[5] = current_minutes;
    current_time_value[6] = current_seconds;
    current_time_value[7] = 0; // Day of the week
    current_time_value[8] = 0; // Fractions
    current_time_value[9] = 0; // TODO: Adjust reasons

    currentTimeCharacteristic.setValue(current_time_value, CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE);
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
    blePeripheral.addAttribute(temperatureCharacteristic);
    blePeripheral.addAttribute(humidityCharacteristic);

    blePeripheral.addAttribute(currentTimeService);
    blePeripheral.addAttribute(currentTimeCharacteristic);

    blePeripheral.setEventHandler(BLEConnected, blePeripheralConnectHandler);
    blePeripheral.setEventHandler(BLEDisconnected, blePeripheralDisconnectHandler);

    // begin initialization
    blePeripheral.begin();

    timer.every(3000, updateReadSensor);
    timer.every(500, updateTimeValue);

    Serial.println(F("BLE LED Peripheral"));
}

void set_current_time() {
    auto raw_time_value = currentTimeCharacteristic.value();
    unsigned char values[CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE];

    memcpy(values, raw_time_value, CURRENT_TIME_CHARACTERISTIC_VALUE_SIZE * sizeof(unsigned char));

    Serial.print("Parsed Date and Time: 0x");
    for (int i = 0; i < sizeof(values) / sizeof(values[0]); i++) {
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
    auto day_of_week = values[7];
    auto fractions_265 = values[8];
    auto adjust_reasons = values[9];

    setTime(hours, minutes, seconds, day, month, year);
}

void loop()
{
    blePeripheral.poll();
    initializeTime();

    if(readFromSensor && dht.read()) {
        auto temperature_reading = dht.readTemperature();
        auto humidity_reading = dht.readHumidity();
        Serial.println("Read the dht");

        if(!isnan(temperature_reading) && !isnan(humidity_reading)) {
            setTempCharacteristicValue(temperature_reading);
            setHumidityCharacteristicValue(humidity_reading);
        } else {
            Serial.println(F("Failed to read from DHT sensor!"));
        }

        readFromSensor = false;
    }

    if (currentTimeCharacteristic.written()) {
        set_current_time();
    }

    if(updateTime) {
        setTimeCharacteristicValue();
        updateTime = false;
    }

    timer.tick();
}