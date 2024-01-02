namespace WirelessCom.Domain.Exceptions;

public class BleDeviceNotFoundException(Guid deviceId) : Exception($"Failed to find device with the id: {deviceId} in the dictionary");