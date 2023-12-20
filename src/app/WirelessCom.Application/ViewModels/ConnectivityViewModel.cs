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
    private IReadOnlyList<BasicBleDevice> _bleDevices = new List<BasicBleDevice>();

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

    private bool _disposed;

    public ConnectivityViewModel(IBleService bleService)
    {
        _bleService = bleService;
        _bleService.HasCorrectPermissions();

        BluetoothStateMessage = _bleService.GetBluetoothState().ToReadableString();

        _bleService.OnBleStateChangedEvent += OnBleStateChanged;
        _bleService.OnDevicesChangedEvent += BleServiceOnOnDevicesChangedEvent;
    }

    public async Task ScanForDevices()
    {
        IsScanning = true;
        await _bleService.ScanForDevices();
        IsScanning = false;
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

        DeviceModalData = new BleDeviceModalData(device, services);
        ServiceModalIsActive = true;
    }

    public async Task ConnectDevice()
    {
        if (DeviceModalData is null)
        {
            return;
        }

        await _bleService.ConnectDeviceByIdAsync(DeviceModalData.Device.Id);

        BleDevices = GetFilteredDevices(_bleService.GetAllBasicBleDevices());
        DeviceModalData = new BleDeviceModalData(
            DeviceModalData.Device,
            await _bleService.GetServicesAsync(DeviceModalData.Device.Id)
        );
    }

    private Task BleServiceOnOnDevicesChangedEvent(object _, IReadOnlyList<BasicBleDevice> devices)
    {
        BleDevices = GetFilteredDevices(devices);
        return Task.CompletedTask;
    }

    public void FilterChanged()
    {
        BleDevices = GetFilteredDevices(_bleService.GetAllBasicBleDevices());
    }

    private IReadOnlyList<BasicBleDevice> GetFilteredDevices(IReadOnlyList<BasicBleDevice> devices)
    {
        if (FilterIsChecked)
        {
            // Todo: This is pure garbage, but it works for now. We need to have a better way of filtering. For example a better device name "RoomSensor-<4 random chars>"
            return devices.Where(
                    x => x.Advertisements.Any(
                        z => z.Type == BleAdvertisementType.UuidsComplete16Bit && ConvertBytesToIntegers(z.Data).Any(y => y == 0x181A)
                    )
                )
                .ToList();
        }

        return devices;
    }

    private static IEnumerable<int> ConvertBytesToIntegers(IReadOnlyList<byte> bytes)
    {
        for (var i = 0; i < bytes.Count; i += 2)
        {
            yield return (bytes[i] << 8) | (i + 1 < bytes.Count ? bytes[i + 1] : 0);
        }
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
            _bleService.OnDevicesChangedEvent -= BleServiceOnOnDevicesChangedEvent;
        }

        BleDevices = null!;

        _disposed = true;
    }

    public async Task<string> Test(Guid device)
    {
        var result = await _bleService.ReadCharacteristicAsync(device, 0x181A.ToBleGuid(), 0x2A6E.ToBleGuid());
        return BitConverter.ToString(result.Bytes.ToArray());
    }
}