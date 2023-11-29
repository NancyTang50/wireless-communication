using WirelessCom.Infrastructure.Extensions;

namespace WirelessCom.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection.RegisterInfrastructureLayer();

        return serviceCollection;
    }
}