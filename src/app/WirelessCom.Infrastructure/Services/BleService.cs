using Plugin.BLE.Abstractions.Contracts;
using WirelessCom.Application.Services;
using BluetoothState = WirelessCom.Domain.Enums.BluetoothState;

namespace WirelessCom.Infrastructure.Services;

public class BleService : IBleService
{
    private readonly IAdapter _adapter;
    private readonly IBluetoothLE _bluetoothLe;

    public BleService(IBluetoothLE bluetoothLe, IAdapter adapter)
    {
        _bluetoothLe = bluetoothLe;
        _adapter = adapter;

        InitOnBleStateChanged();
    }

    public event IBleService.OnBleStateChanged? OnBleStateChangedEvent;

    public async Task<bool> HasCorrectPermissions()
    {
        if (await Permissions.RequestAsync<Permissions.Bluetooth>() == PermissionStatus.Granted)
        {
            return true;
        }

        AppInfo.ShowSettingsUI();
        return false;
    }

    private void InitOnBleStateChanged()
    {
        _bluetoothLe.StateChanged += (_, args) => OnBleStateChangedEvent?.Invoke(this, (BluetoothState)args.NewState);
    }
}