﻿using CommunityToolkit.Mvvm.ComponentModel;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Extensions;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Application.ViewModels;

public partial class ConnectivityViewModel : BaseViewModel
{
    private readonly IBleService _bleService;

    [ObservableProperty]
    private IReadOnlyList<BareBleAdvertisement>? _bleAdvertisements = new List<BareBleAdvertisement>();

    [ObservableProperty]
    private IReadOnlyList<BasicBleDevice> _bleDevices = new List<BasicBleDevice>();

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
        await _bleService.ScanForDevices().ConfigureAwait(false);
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
        BleAdvertisements = null;
    }

    public void OpenServicesModal(Guid deviceId)
    {
        BleAdvertisements = _bleService.GetBareBleAdvertisements(deviceId);
        ServiceModalIsActive = true;
    }
}