namespace WirelessCom.Domain.Models;

public record BasicBleDevice(
    Guid Id,
    string Name,
    bool IsConnected,
    int Rssi,
    IReadOnlyList<BareBleAdvertisement> Advertisements,
    IReadOnlyList<BasicBleService>? BleServices
);