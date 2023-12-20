using WirelessCom.Application.Database.Repositories;
using WirelessCom.Domain.Models.Entities;
using WirelessCom.Infrastructure.Persistence;

namespace WirelessCom.Infrastructure.Database.Repositories;

public class TemperatureRepository : Repository<RoomClimateReading>, IRoomClimateReadingRepository
{
    public TemperatureRepository(ClimateDbContext context) : base(context)
    {
    }
}