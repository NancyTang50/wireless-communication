﻿using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using WirelessCom.Application.Database;
using WirelessCom.Application.Database.Repositories;
using WirelessCom.Application.Extensions;
using WirelessCom.Application.Services;
using WirelessCom.Infrastructure.Database;
using WirelessCom.Infrastructure.Database.Repositories;
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
        serviceCollection.AddScoped(
            _ =>
            {
                var adapter = CrossBluetoothLE.Current.Adapter;
                adapter.ScanMode = ScanMode.LowLatency;

                return adapter;
            }
        );

        serviceCollection.AddSingleton<IBleService, LockedBleService>();

        serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
        serviceCollection.AddTransient<IRoomClimateReadingRepository, RoomClimateReadingRepository>();

        serviceCollection.AddTransient<IBleRoomSensorNamingService, BleRoomSensorNamingService>();
        serviceCollection.AddTransient<IToastService, ToastService>();

        return serviceCollection;
    }
}