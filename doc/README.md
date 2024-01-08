# Wireless Communication Report <!-- omit in toc -->

- [Introduction](#introduction)
- [The goal of the system](#the-goal-of-the-system)
- [The roles of the different devices](#the-roles-of-the-different-devices)
  - [Server - Nordic-nRF52 \& Raspberry Pi](#server---nordic-nrf52--raspberry-pi)
  - [Client - Android phone](#client---android-phone)
- [Services \& characteristic](#services--characteristic)
  - [Environmental sensing service](#environmental-sensing-service)
    - [Encoding of the bytes](#encoding-of-the-bytes)
    - [Temperature Characteristic](#temperature-characteristic)
    - [Humidity Characteristic](#humidity-characteristic)
  - [Current time service](#current-time-service)
    - [Encoding of the bytes](#encoding-of-the-bytes-1)
    - [Current time characteristic](#current-time-characteristic)
- [How to use the system](#how-to-use-the-system)
- [Coding explanation](#coding-explanation)
  - [Android phone](#android-phone)
  - [Raspberry Pi](#raspberry-pi)
  - [Nordic-nrF-52840](#nordic-nrf-52840)
- [Testing](#testing)
- [Problems we encountered](#problems-we-encountered)
  - [Finding examples of the Raspberry Pi](#finding-examples-of-the-raspberry-pi)

## Introduction

For this project we developed a cross-platform app to show the temperature and the humidity of the locations where the sensors are. This is interesting for detecting variations in temperature and humidity across different places. To achieve this, we are using the DHT-22 sensor to measure the temperature and humidity for the Nordic-nRF52 board and Raspberry Pi and we will show the  current time of these two peripherals.

## The goal of the system

The goal of our system is to monitor temperature and the humidity across different places. So that the user can see the history of the humidity and temperature of the sensor for each device.

## The roles of the different devices

In this chapter we will explain the roles of the different devices in the chapters.

### Server - Nordic-nRF52 & Raspberry Pi

Raspberry Pi and the Nordic-nrF-52840 are both using the DHT22 sensors, because these two devices will act as peripherals. To read the temperature and humidity values from the DHT22 sensors, two separate applications will be developed. One application will be developed in Rust for the Raspberry Pi. And another application will be developed in C++ with PlatformIO.

### Client - Android phone

The Android phone will act as a BLE central. The .NET MAUI cross-platform app will be developed to manage the receiving data of the connected BLE devices. The received data will be stored in a local SQL lite database and shown in a couple charts.

## Services & characteristic

The Nordic-nrF52 and Raspberry Pi act as peripherals. These peripherals supports two services. These two services are environmental sensing service, and the current time service.

### Environmental sensing service

The environmental sensing service supports multiple optional sensors. Our implementation only supports the temperature and humidity.

#### Encoding of the bytes

The temperature and the humidty are both floating point values, because of this they both have the same encoding. First the value needs to be multiplied by 100, then this value needs to be converted into a signed 16 bit number. Then this number can be send in little endian format. The psuedo code of this can be found below:

```python
some_float_value = 22.5
signed_16_bits = (signed 16 bits)(22.5 * 100)
low_byte = signed_16_bits & 0xFF
high_byte = (signed_16_bits >> 8) & 0xFF
[low_bytes, high_byte]
```

#### Temperature Characteristic

In our system, the temperature characteristic supports reading and notifiying the current Celsius temperature of the DHT22 sensor.

#### Humidity Characteristic

In our system, the humidity characteristic supports reading and notifiying the current humidity percentage of the DHT22 sensor.

### Current time service

The current time service displays the current time of the peripheral device. The current time service has a current time characteristic.

#### Encoding of the bytes

The current time is encoding using in the following way:

|byte|1|2|3|4|5|6|7|8|9|10|
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
|**Description**|year low bytes|year high bytes | month | day | hours | minutes | seconds | day of the week | fractions 256 | adjustment reason |

All values are 1 bytes, except for the year. The year needs to be little endian encoded.

#### Current time characteristic

In our system, the current time characteristic supports reading and writing the current time. If you read the time before writing the current time, the response may be inaccurate depending on the platform.

## How to use the system

First follow these installation guides below

The wiring of the DHT22 sensor depends on the platform. The wiring of the Raspberry Pi can be found [here](https://github.com/NancyTang50/wireless-communication/blob/master/src/pi/README.md) and the Nordic-nrF52840 can be found [here](https://github.com/NancyTang50/wireless-communication/blob/master/src/platformio/README.MD).

## Coding explanation

### Android phone

### Raspberry Pi

The Raspberry Pi's program is written in the language Rust. A Rust binary project always contains a main.rs file, where a main function can be found. The main function is the starting point of a Rust project. The main function initializes the peripheral with the services. 

To create a BLE peripheral program for the Raspberry Pi the package [Bluster](https://docs.rs/bluster/0.2.0/bluster/index.html) is used. The Bluser package uses the official Bluetooth program of Linux called [BlueZ](https://www.bluez.org/)

The services are defined in the gatt folder. The gatt folder contains a characteristic folder that contains generic characteristic code. The other folders are the services that are supported by the Raspberry Pi. The services are defined in the mod.rs file, there are create the service methods. These methods create the characteristics for the service.

The characteristics all contain a create characteristic function. The create characteristic function creates a characteristic handler that is used to handle incoming messages. The handling of these message will be executed in a new thread. After setting up the handling of the messages the bluster characteristic returned. To allow the characteristic handler to handle the messages, the characteristic needs to implement the GattEventHandler trait. This will define handle request method, where all incoming message the the characteristic need to be handled. To allow for notify subscriptions the characteristic also needs implement the SensorDataHandler trait. This allows the characteristic to receive updates of the DHT22 sensor.

### Nordic-nrF-52840

The Nordic-nrF-52840's program is written in the language C with PlatformIO using the Arduino library. This resulted that the project always have a main.cpp containing two standard functions called setup() and loop(). The setup function is the starting point of a PlatformIO project. The setup function initialize the services and add the characteristics to the Peripheral. 

To create a BLE peripheral program for the Nordic-nrF52840 the package [BLEPeripheral](https://registry.platformio.org/libraries/sandeepmistry/BLEPeripheral) is used. 

The services are defined in the main.cpp.

## Testing


## Problems we encountered

This chapter explains the problems and difficulties we encounterd during this project.

- Nordic mbed did not work, platform io
- Nordic DHT library timing issue

### Finding examples of the Raspberry Pi

Initialy we had alot of difficulty finding an example of BLE peripheral implemetation in Rust. This was due to the constraint that we did not want to use embedded Rust, because we never used embedded Rust. After some research we found the package [Bluster](https://docs.rs/bluster/0.2.0/bluster/index.html), but the documentation of this package is empty. After searching in the GIT repository we found the [it_advertises_gatt](https://github.com/dfrankland/bluster/blob/e928dd6491d4cc3c42164b6594a4d584b240c8e1/tests/peripheral.rs#L26C19-L26C19) test, where a basic application is created. Later we found an issue that asked the maintainer for documentation/examples, where the maintainer linked to a complete application called [bleboard](https://github.com/dfrankland/bleboard/tree/master).

- BLE library app

