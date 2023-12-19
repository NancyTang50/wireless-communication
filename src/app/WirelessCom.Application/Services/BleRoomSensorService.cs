using WirelessCom.Domain.Services;

namespace WirelessCom.Application.Services;

public class BleRoomSensorService : IBleRoomSensorService
{
    private readonly IBleService _bleService;

    public BleRoomSensorService(IBleService bleService)
    {
        _bleService = bleService;
    }
}