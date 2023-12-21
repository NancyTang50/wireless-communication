﻿using WirelessCom.Application.Extensions;

namespace WirelessCom.Application;

public static class BleServiceDefinitions
{
    public static class TimeService
    {
        public const int ServiceIdPrefix = 0x1805;
        public static Guid ServiceGuid { get; } = ServiceIdPrefix.ToBleGuid();

        public const int CurrentTimeCharacteristicIdPrefix = 0x2A2B;
        public static Guid CurrentTimeCharacteristicGuid { get; } = CurrentTimeCharacteristicIdPrefix.ToBleGuid();
    }

    public static class EnvironmentalService
    {
        public const int ServiceIdPrefix = 0x181A;
        public static Guid ServiceGuid { get; } = ServiceIdPrefix.ToBleGuid();

        public const int TemperatureCharacteristicIdPrefix = 0x2A6E;
        public static Guid TemperatureCharacteristicGuid { get; } = TemperatureCharacteristicIdPrefix.ToBleGuid();

        public const int HumidityCharacteristicIdPrefix = 0x2A6F;
        public static Guid HumidityCharacteristicGuid { get; } = HumidityCharacteristicIdPrefix.ToBleGuid();
    }
}