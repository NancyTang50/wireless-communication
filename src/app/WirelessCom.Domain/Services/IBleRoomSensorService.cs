using WirelessCom.Domain.Models.Entities;

namespace WirelessCom.Domain.Services;

public interface IBleRoomSensorService
{
    /// <summary>
    ///     Delegate for the <see cref="OnNewReadingReceivedEvent" /> event.
    /// </summary>
    public delegate Task? OnNewReadingReceived(object source, RoomClimateReading reading);

    /// <summary>
    ///     Event that is raised when a new reading is received.
    /// </summary>
    public event OnNewReadingReceived OnNewReadingReceivedEvent;
    
    /// <summary>
    ///     Scans for room sensors.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    ///     Return after the scan is complete.
    /// </returns>
    Task ScanForRoomSensors(CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     Reads the room climate of the specified device.
    /// </summary>
    /// <param name="deviceId">The device identifier of the room sensor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    ///     The task result contains the room climate reading.
    /// </returns>
    Task<RoomClimateReading> ReadRoomClimate(Guid deviceId, CancellationToken cancellationToken = default);
}