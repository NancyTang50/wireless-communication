using Plugin.BLE;
using WirelessCom.Application.Extensions;
using WirelessCom.Application.Services;
using WirelessCom.Infrastructure.Persistence.Extensions;
using WirelessCom.Infrastructure.Services;

namespace WirelessCom.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.RegisterApplicationLayer();
        serviceCollection.RegisterPersistenceLayer();

        serviceCollection.AddScoped(_ => CrossBluetoothLE.Current);
        serviceCollection.AddScoped(_ => CrossBluetoothLE.Current.Adapter);

        serviceCollection.AddSingleton<IBleService, BleService>();

        return serviceCollection;
    }
}