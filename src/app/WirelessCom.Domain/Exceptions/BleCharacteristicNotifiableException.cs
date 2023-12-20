namespace WirelessCom.Domain.Exceptions;

public class BleCharacteristicNotifiableException(Guid guid) : Exception($"Characteristic is not notifiable {guid}");