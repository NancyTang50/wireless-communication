using WirelessCom.Domain.Enums;

namespace WirelessCom.Application.Services;

public interface IBleService
{
    public delegate void OnBleStateChanged(object source, BluetoothState bluetoothState);
    public event OnBleStateChanged OnBleStateChangedEvent;

    Task<bool> HasCorrectPermissions();
    BluetoothState GetBluetoothState();
}