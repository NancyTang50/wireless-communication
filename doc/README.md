# Wireless Communication Report <!-- omit in toc -->

- [Introduction](#introduction)
- [The goal of the system](#the-goal-of-the-system)
- [The roles of the different devices](#the-roles-of-the-different-devices)
  - [Server - Nordic-nRF52 \& Raspberry Pi](#server---nordic-nrf52--raspberry-pi)
  - [Client - Android phone](#client---android-phone)
- [Services \& characteristic](#services--characteristic)
  - [Environmental sensing service](#environmental-sensing-service)
    - [Temperature Characteristic](#temperature-characteristic)
    - [Humidity Characteristic](#humidity-characteristic)
  - [Current time service](#current-time-service)
- [How to use the system](#how-to-use-the-system)
- [Coding explanation](#coding-explanation)
  - [Android phone](#android-phone)
  - [PI](#pi)
  - [Nordic](#nordic)
- [Testing](#testing)

## Introduction

For this project we developed a cross-platform app to show the temperature and the humidity of the locations where the sensors are. This is interesting for detecting variations in temperature and humidity across different places. To achieve this, we
are using the DHT-22 sensor to measure the temperature and humidity for the Nordic-nRF52 board and Raspberry Pi.

## The goal of the system


## The roles of the different devices
In this chapter we will explain the roles of the different devices in the chapters.

### Server - Nordic-nRF52 & Raspberry Pi

Raspberry Pi and the Nordic-nrF-52840 are both using the DHT22 sensors, because these two devices will act as peripherals. To read the temperature and humidity values from the DHT22 sensors, two separate applications will be developed. One application will be developed in Rust for the Raspberry Pi. And the other application will be developed in C++ with PlatformIO.


### Client - Android phone
The Android phone will act as a BLE central. The .NET MAUI cross-platform app will be developed to manage the receiving data of the connected BLE devices. The received data will be stored in a local SQL lite database and shown in a couple charts.

## Services & characteristic

The peripherals support two services, the environmental sensing service and the current time service.

### Environmental sensing service

The environmental sensing service supports multiple optional sensors. Our implementation only supports the temperature and humidity.

#### Temperature Characteristic

The temperature characteristic support the reading and notifying of the current Celsius temperature.

The temperature value is encode

#### Humidity Characteristic

The humidity characteristic support the reading and notifying of the current humidity percentage.

### Current time service

The current time service allows the device to display the current time.
This only has the current time characteristic. This characteristic allows you to read the time, and write the time. Before reading, a write has to be performed to set the correct system time.   

## How to use the system


## Coding explanation

### Android phone

### PI



### Nordic


## Testing


