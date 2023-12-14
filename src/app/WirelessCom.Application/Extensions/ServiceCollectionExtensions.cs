using Microsoft.Extensions.DependencyInjection;

namespace WirelessCom.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplicationLayer(this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }
}