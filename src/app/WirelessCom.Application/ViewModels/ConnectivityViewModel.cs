using CommunityToolkit.Mvvm.ComponentModel;
using WirelessCom.Application.Models;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Extensions;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Application.ViewModels;

public partial class ConnectivityViewModel : BaseViewModel
{
    private readonly IBleService _bleService;

    [ObservableProperty]
    private IReadOnlyList<BasicBleDevice> _bleDevices = new List<BasicBleDevice>();

    [ObservableProperty]
    private BleDeviceModalData? _deviceModalData;

    [ObservableProperty]
    private string _bluetoothStateMessage = string.Empty;

    [ObservableProperty]
    private bool _serviceModalIsActive;

    public ConnectivityViewModel(IBleService bleService)
    {
        _bleService = bleService;
        _bleService.HasCorrectPermissions();

        BluetoothStateMessage = _bleService.GetBluetoothState().ToReadableString();
        _bleService.OnBleStateChangedEvent += OnBleStateChanged;
    }

    public async Task ScanForDevices()
    {
        _bleService.OnDevicesChangedEvent += BleServiceOnOnDevicesChangedEvent;
        await _bleService.ScanForDevices();
        _bleService.OnDevicesChangedEvent -= BleServiceOnOnDevicesChangedEvent;
        return;

        Task BleServiceOnOnDevicesChangedEvent(object _, IReadOnlyList<BasicBleDevice> devices)
        {
            BleDevices = devices;
            return Task.CompletedTask;
        }
    }

    private void OnBleStateChanged(object source, BluetoothState bluetoothState)
    {
        BluetoothStateMessage = bluetoothState.ToReadableString();
    }

    public void CloseServicesModal()
    {
        ServiceModalIsActive = false;
        DeviceModalData = null;
    }

    public async Task OpenServicesModal(Guid deviceId)
    {
        var device = BleDevices.First(x => x.Id == deviceId);
        var services = device.IsConnected
            ? await _bleService.GetServicesAsync(deviceId)
            : new List<BasicBleService>();

        DeviceModalData = new BleDeviceModalData(device, _bleService.GetBareBleAdvertisements(deviceId), services);
        ServiceModalIsActive = true;
    }

    public async Task ConnectDevice()
    {
        if (DeviceModalData is null)
        {
            return;
        }

        await _bleService.ConnectDeviceByIdAsync(DeviceModalData.Device.Id);

        BleDevices = _bleService.GetAllBasicBleDevices();
        DeviceModalData = new BleDeviceModalData(
            DeviceModalData.Device,
            _bleService.GetBareBleAdvertisements(DeviceModalData.Device.Id),
            await _bleService.GetServicesAsync(DeviceModalData.Device.Id)
        );
    }
}