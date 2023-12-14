using WirelessCom.Infrastructure.Extensions;

namespace WirelessCom.UI.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection.RegisterInfrastructureLayer();

        return serviceCollection;
    }
}