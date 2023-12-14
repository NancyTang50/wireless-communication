namespace WirelessCom.Domain.Exceptions;

public class RequireBleConnectionException(Guid guid) : Exception($"Not currently connected to this device {guid}");