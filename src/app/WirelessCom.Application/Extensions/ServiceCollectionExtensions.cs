using Microsoft.Extensions.DependencyInjection;
using WirelessCom.Application.Services;
using WirelessCom.Application.ViewModels;
using WirelessCom.Domain.Services;

namespace WirelessCom.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplicationLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.Scan(
            scan => scan
                .FromAssemblyOf<BaseViewModel>()
                .AddClasses(classes => classes.AssignableTo<BaseViewModel>())
                .AsSelf()
                .WithTransientLifetime()
        );

        serviceCollection.AddSingleton<IBleRoomSensorService, BleRoomSensorService>();

        return serviceCollection;
    }
}