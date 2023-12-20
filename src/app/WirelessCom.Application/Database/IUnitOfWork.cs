using WirelessCom.Application.Database.Repositories;

namespace WirelessCom.Application.Database;

public interface IUnitOfWork
{
    /// <summary>
    ///     The <see cref="IRoomClimateReadingRepository" /> containing all <see cref="RoomClimateReading" /> entities.
    /// </summary>
    public IRoomClimateReadingRepository RoomClimateReading { get; init; }

    /// <summary>
    ///     Saves changes to the database.
    /// </summary>
    /// <returns>
    ///     The number of state entries written to the database.
    /// </returns>
    public int SaveChanges();

    /// <summary>
    ///     Saves changes to the database.
    /// </summary>
    /// <returns>
    ///     The number of state entries written to the database.
    /// </returns>
    public Task<int> SaveChangesAsync();
}