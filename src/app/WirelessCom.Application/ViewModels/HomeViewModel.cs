using CommunityToolkit.Mvvm.ComponentModel;
using WirelessCom.Application.Database;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Entities;
using WirelessCom.Domain.Services;

namespace WirelessCom.Application.ViewModels;

public partial class HomeViewModel : BaseViewModel, IDisposable
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBleRoomSensorService _roomSensorService;
    private readonly IBleRoomSensorNamingService _bleRoomSensorNamingService;

    [ObservableProperty]
    private List<LineChartData> _data = [];

    [ObservableProperty]
    private bool _sensorNameModalIsActive;

    [ObservableProperty]
    private string _newSensorName = string.Empty;

    private Guid _selectedDeviceGuid;
    private bool _disposed;

    public HomeViewModel(IUnitOfWork unitOfWork, IBleRoomSensorService roomSensorService, IBleRoomSensorNamingService bleRoomSensorNamingService)
    {
        _unitOfWork = unitOfWork;
        _roomSensorService = roomSensorService;
        _bleRoomSensorNamingService = bleRoomSensorNamingService;

        _roomSensorService.OnNewReadingReceivedEvent += OnNewReadingReceived;
    }

    public async Task OnInitializedAsync()
    {
        await UpdateRoomSensorReadingsAsync();
    }

    public void CloseServicesModal()
    {
        SensorNameModalIsActive = false;
    }

    public void OpenServicesModal(Guid deviceId)
    {
        _selectedDeviceGuid = deviceId;
        SensorNameModalIsActive = true;
    }

    public async Task SetSensorName()
    {
        _bleRoomSensorNamingService.SetName(_selectedDeviceGuid, NewSensorName);
        SensorNameModalIsActive = false;
        await UpdateRoomSensorReadingsAsync();
        CloseServicesModal();
    }

    private async Task UpdateRoomSensorReadingsAsync()
    {
        var tempData = await _unitOfWork.RoomClimateReading.WhereAsync(x => x.Timestamp > DateTime.Now.AddHours(-1));
        Data = tempData
            .GroupBy(x => x.DeviceId)
            .Select(x => GetLineChartData(x.Key, x.OrderBy(z => z.Timestamp).ToList()))
            .ToList();
    }

    private Task OnNewReadingReceived(object source, RoomClimateReading reading)
    {
        var lineChartData = Data.FirstOrDefault(x => x.DeviceId == reading.DeviceId);

        if (lineChartData is null)
        {
            lineChartData = GetLineChartData(reading.DeviceId, []);
            Data.Add(lineChartData);
        }

        lineChartData.Readings.Add(reading);

        Data = Data.ToList();
        return Task.CompletedTask;
    }

    private LineChartData GetLineChartData(Guid id, List<RoomClimateReading> readings)
    {
        return new LineChartData(id, readings, _bleRoomSensorNamingService.GetName(id));
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
            _roomSensorService.OnNewReadingReceivedEvent -= OnNewReadingReceived;
        }

        Data = null!;

        _disposed = true;
    }
}