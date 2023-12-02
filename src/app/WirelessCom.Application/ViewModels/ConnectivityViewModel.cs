using CommunityToolkit.Mvvm.ComponentModel;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Enums;
using WirelessCom.Domain.Extensions;

namespace WirelessCom.Application.ViewModels;

public partial class ConnectivityViewModel : BaseViewModel
{
    private readonly IBleService _bleService;

    public ConnectivityViewModel(IBleService bleService)
    {
        _bleService = bleService;
        _bleService.HasCorrectPermissions();
        
        _bleService.OnBleStateChangedEvent += OnBleStateChanged;
    }

    private void OnBleStateChanged(object source, BluetoothState bluetoothstate)
    {
        BluetoothStateMessage = bluetoothstate.ToReadableString();
    }

    [ObservableProperty]
    private string _bluetoothStateMessage = "Unknown";
}