using CommunityToolkit.Mvvm.ComponentModel;
using WirelessCom.Application.Database;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Entities;
using WirelessCom.Domain.Services;

namespace WirelessCom.Application.ViewModels;

public partial class HomeViewModel : BaseViewModel, IDisposable
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBleRoomSensorService _roomSensorService;

    [ObservableProperty]
    private List<LineChartData> _data = new();

    private bool _disposed;

    public HomeViewModel(IUnitOfWork unitOfWork, IBleRoomSensorService roomSensorService)
    {
        _unitOfWork = unitOfWork;
        _roomSensorService = roomSensorService;

        _roomSensorService.OnNewReadingReceivedEvent += OnNewReadingReceived;
    }

    public async Task OnInitializedAsync()
    {
        var tempData = await _unitOfWork.RoomClimateReading.WhereAsync(x => x.Timestamp > DateTime.Now.AddHours(-1));
        Data = tempData.GroupBy(x => x.DeviceId).Select(x => new LineChartData(x.Key, x.OrderBy(z => z.Timestamp).ToList())).ToList();
    }

    private Task OnNewReadingReceived(object source, RoomClimateReading reading)
    {
        var lineChartData = Data.FirstOrDefault(x => x.DeviceId == reading.DeviceId);

        if (lineChartData is null)
        {
            lineChartData = new LineChartData(reading.DeviceId, new List<RoomClimateReading>());
            Data.Add(lineChartData);
        }

        lineChartData.Readings.Add(reading);

        Data = Data.ToList();
        return Task.CompletedTask;
    }

    public async Task ReadRoomSensor(Guid deviceId)
    {
        await _roomSensorService.ReadRoomClimate(deviceId);
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