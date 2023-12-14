using WirelessCom.Application.Extensions;
using WirelessCom.Infrastructure.Persistence.Extensions;

namespace WirelessCom.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.RegisterApplicationLayer();
        serviceCollection.RegisterPersistenceLayer();

        return serviceCollection;
    }
}