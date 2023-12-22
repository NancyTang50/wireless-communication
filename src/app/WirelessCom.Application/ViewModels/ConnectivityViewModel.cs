using CommunityToolkit.Mvvm.ComponentModel;
using WirelessCom.Application.Extensions;
using WirelessCom.Application.Models;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Extensions;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Application.ViewModels;

public partial class ConnectivityViewModel : BaseViewModel, IDisposable
{
    private readonly IBleService _bleService;

    [ObservableProperty]
    private List<BasicBleDevice> _bleDevices = new();

    [ObservableProperty]
    private BleDeviceModalData? _deviceModalData;

    [ObservableProperty]
    private string _bluetoothStateMessage = string.Empty;

    [ObservableProperty]
    private bool _serviceModalIsActive;

    [ObservableProperty]
    private bool _filterIsChecked;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isConnecting;

    private bool _disposed;

    public ConnectivityViewModel(IBleService bleService)
    {
        _bleService = bleService;
        _bleService.HasCorrectPermissions();

        BluetoothStateMessage = _bleService.GetBluetoothState().ToReadableString();

        _bleService.OnBleStateChangedEvent += OnBleStateChanged;
        _bleService.OnDevicesChangedEvent += BleServiceOnDevicesChangedEvent;
    }

    public async Task ScanForDevices()
    {
        try
        {
            IsScanning = true;
            await _bleService.ScanForDevices();
        }
        finally
        {
            IsScanning = false;
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
            ? device.BleServices ?? await _bleService.GetServicesAsync(deviceId)
            : new List<BasicBleService>();

        DeviceModalData = new BleDeviceModalData(device, services);
        ServiceModalIsActive = true;
    }

    public async Task ConnectDevice()
    {
        if (DeviceModalData is null)
        {
            return;
        }

        try
        {
            IsConnecting = true;
            await _bleService.ConnectDeviceByIdAsync(DeviceModalData.Device.Id);

            var services = await _bleService.GetServicesAsync(DeviceModalData.Device.Id);
            var device = BleDevices.First(x => x.Id == DeviceModalData.Device.Id);
            var index = BleDevices.IndexOf(device);

            BleDevices[index] = device with { IsConnected = true, BleServices = services };
            BleDevices = GetFilteredDevices(_bleService.GetAllBasicBleDevices());
            DeviceModalData = new BleDeviceModalData(BleDevices[index], services);
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private Task BleServiceOnDevicesChangedEvent(object _, IReadOnlyList<BasicBleDevice> devices)
    {
        BleDevices = GetFilteredDevices(devices.ToList());
        return Task.CompletedTask;
    }

    public void FilterChanged()
    {
        BleDevices = GetFilteredDevices(_bleService.GetAllBasicBleDevices());
    }

    private List<BasicBleDevice> GetFilteredDevices(List<BasicBleDevice> devices)
    {
        if (FilterIsChecked)
        {
            return devices
                .FilterByServiceId(BleServiceDefinitions.EnvironmentalService.ServiceIdPrefix, BleServiceDefinitions.TimeService.ServiceIdPrefix)
                .ToList();
        }

        return devices;
    }

    public void Dispose()
    {
        Dispose(true);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _bleService.OnBleStateChangedEvent -= OnBleStateChanged;
            _bleService.OnDevicesChangedEvent -= BleServiceOnDevicesChangedEvent;
        }

        BleDevices = null!;

        _disposed = true;
    }
}