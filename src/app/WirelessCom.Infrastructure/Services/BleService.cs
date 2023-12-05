﻿using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using WirelessCom.Application.Caching;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Exceptions;
using WirelessCom.Domain.Models;
using BluetoothState = WirelessCom.Domain.Models.Enums.BluetoothState;

namespace WirelessCom.Infrastructure.Services;

public class BleService : IBleService
{
    private readonly GenericConcurrentDictionary<Guid, IDevice> _devices = new();
    private readonly IAdapter _adapter;
    private readonly IBluetoothLE _bluetoothLe;

    public BleService(IBluetoothLE bluetoothLe, IAdapter adapter)
    {
        _bluetoothLe = bluetoothLe;
        _adapter = adapter;

        InitOnBleStateChanged();
        InitOnDevicesChangedEvent();
    }

    /// <inheritdoc />
    public event IBleService.OnBleStateChanged? OnBleStateChangedEvent;

    /// <inheritdoc />
    public event IBleService.OnDevicesChanged? OnDevicesChangedEvent;

    /// <inheritdoc />
    public async Task<bool> HasCorrectPermissions()
    {
        if (await Permissions.RequestAsync<Permissions.Bluetooth>().ConfigureAwait(false) == PermissionStatus.Granted)
        {
            return true;
        }

        AppInfo.ShowSettingsUI();
        return false;
    }

    /// <inheritdoc />
    public BluetoothState GetBluetoothState()
    {
        return (BluetoothState)_bluetoothLe.State;
    }

    /// <inheritdoc />
    public async Task ScanForDevices(Guid[]? guids, CancellationToken cancellationToken = default)
    {
        var scanFilterOptions = new ScanFilterOptions();
        if (guids != null)
        {
            scanFilterOptions.ServiceUuids = guids;
        }

        _adapter.DeviceDiscovered += (_, a) => _devices.Add(a.Device.Id, a.Device);
        await _adapter.StartScanningForDevicesAsync(scanFilterOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<BasicBleService>> GetServicesAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var device = _devices.Get(deviceId) ?? throw new BleDeviceNotFoundException(deviceId);
            var services = await device.GetServicesAsync(cancellationToken).ConfigureAwait(false);
            return services.Select(service => new BasicBleService(service.Id, service.Device.Id, service.Name, service.IsPrimary)).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    private void InitOnBleStateChanged()
    {
        _bluetoothLe.StateChanged += (_, args) => OnBleStateChangedEvent?.Invoke(this, (BluetoothState)args.NewState);
    }

    private void InitOnDevicesChangedEvent()
    {
        _devices.ItemAdded += (_, _) => OnDevicesChangedEvent?.Invoke(this, GetAllBasicBleDevices());
        _devices.ItemRemoved += (_, _) => OnDevicesChangedEvent?.Invoke(this, GetAllBasicBleDevices());
    }

    private List<BasicBleDevice> GetAllBasicBleDevices()
    {
        return _devices.GetAll().Select(device => new BasicBleDevice(device.Key, device.Value.Name)).ToList();
    }
}