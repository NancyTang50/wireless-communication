using WirelessCom.Application.Database.Repositories;
using WirelessCom.Domain.Models.Entities;
using WirelessCom.Infrastructure.Persistence;

namespace WirelessCom.Infrastructure.Database.Repositories;

public class RoomClimateReadingRepository : Repository<RoomClimateReading>, IRoomClimateReadingRepository
{
    public RoomClimateReadingRepository(ClimateDbContext context) : base(context)
    {
    }
}