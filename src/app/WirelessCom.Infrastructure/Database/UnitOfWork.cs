using WirelessCom.Application.Database;
using WirelessCom.Application.Database.Repositories;
using WirelessCom.Infrastructure.Persistence;

namespace WirelessCom.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly ClimateDbContext _context;

    public UnitOfWork(ClimateDbContext context, IRoomClimateReadingRepository roomClimateReadingRepository)
    {
        _context = context;
        RoomClimateReading = roomClimateReadingRepository;
    }

    /// <inheritdoc />
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public IRoomClimateReadingRepository RoomClimateReading { get; init; }
}