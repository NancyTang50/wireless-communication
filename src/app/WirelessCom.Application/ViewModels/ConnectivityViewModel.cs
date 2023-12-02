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
        
        BluetoothStateMessage = _bleService.GetBluetoothState().ToReadableString();
        _bleService.OnBleStateChangedEvent += OnBleStateChanged;
    }
    
    [ObservableProperty]
    private string _bluetoothStateMessage;

    private void OnBleStateChanged(object source, BluetoothState bluetoothState)
    {
        BluetoothStateMessage = bluetoothState.ToReadableString();
    }
}