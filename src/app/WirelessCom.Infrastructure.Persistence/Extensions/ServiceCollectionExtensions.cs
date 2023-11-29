using Microsoft.Extensions.DependencyInjection;

namespace WirelessCom.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPersistenceLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<ClimateDbContext>();

        return serviceCollection;
    }
}