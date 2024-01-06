using CommunityToolkit.Mvvm.ComponentModel;
using WirelessCom.Application.Extensions;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Extensions;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Application.ViewModels;

public partial class ConnectivityViewModel : BaseViewModel, IDisposable
{
    private readonly IBleService _bleService;

    [ObservableProperty]
    private static List<BasicBleDevice> _bleDevices = new();

    [ObservableProperty]
    private string _bluetoothStateMessage = string.Empty;

    private bool _disposed;

    [ObservableProperty]
    private BasicBleDevice? _selectedDevice;

    [ObservableProperty]
    private bool _serviceModalIsActive, _filterIsChecked, _isScanning, _isConnecting;

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

    public void CloseServicesModal()
    {
        ServiceModalIsActive = false;
    }

    public void OpenServicesModal(Guid deviceId)
    {
        SelectedDevice = BleDevices.First(x => x.Id == deviceId);
        ServiceModalIsActive = true;
    }

    public async Task ConnectDevice()
    {
        if (SelectedDevice is null)
        {
            return;
        }

        try
        {
            IsConnecting = true;
            await _bleService.ConnectDeviceByIdAsync(SelectedDevice.Id);

            var services = await _bleService.GetServicesAsync(SelectedDevice.Id);
            var device = BleDevices.First(x => x.Id == SelectedDevice.Id);
            var index = BleDevices.IndexOf(device);

            BleDevices[index] = device with { IsConnected = true, Services = services.ToList() };
            SelectedDevice = BleDevices.First(x => x.Id == SelectedDevice.Id);
            UpdateDeviceList();
        }
        finally
        {
            IsConnecting = false;
        }
    }

    public async Task DisconnectDevice()
    {
        if (SelectedDevice is null)
        {
            return;
        }

        try
        {
            IsConnecting = true;
            await _bleService.DisconnectDeviceByIdAsync(SelectedDevice.Id);

            var device = BleDevices.First(x => x.Id == SelectedDevice.Id);
            var index = BleDevices.IndexOf(device);

            BleDevices[index] = device with { IsConnected = false };
            SelectedDevice = BleDevices.First(x => x.Id == SelectedDevice.Id);
            UpdateDeviceList();
        }
        finally
        {
            IsConnecting = false;
        }
    }

    public void FilterChanged()
    {
        UpdateDeviceList();
    }

    private void UpdateDeviceList(IEnumerable<BasicBleDevice>? devices = null)
    {
        var filteredDevices = GetFilteredDevices(devices?.ToList() ?? _bleService.GetAllBasicBleDevices());

        // Keep the services of the devices that are already in the list. Because the library doesn't keep the services
        foreach (var existingDevice in BleDevices.Where(x => x.Services is not null))
        {
            var device = filteredDevices.FirstOrDefault(x => x.Id == existingDevice.Id);
            if (device is not null)
            {
                filteredDevices[filteredDevices.IndexOf(device)] = existingDevice with
                {
                    Services = existingDevice.Services
                };
            }
        }

        BleDevices = filteredDevices;
    }

    private List<BasicBleDevice> GetFilteredDevices(List<BasicBleDevice> devices)
    {
        return FilterIsChecked ? devices.Where(x => x.IsRoomSensor()).ToList() : devices;
    }

    private void OnBleStateChanged(object source, BluetoothState bluetoothState)
    {
        BluetoothStateMessage = bluetoothState.ToReadableString();
    }

    private Task BleServiceOnDevicesChangedEvent(object _, IEnumerable<BasicBleDevice> devices)
    {
        UpdateDeviceList(devices);
        return Task.CompletedTask;
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

        _disposed = true;
    }
}