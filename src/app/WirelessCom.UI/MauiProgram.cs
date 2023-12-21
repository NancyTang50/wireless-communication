using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WirelessCom.Domain.Services;
using WirelessCom.Infrastructure.Persistence;
using WirelessCom.UI.Extension;

namespace WirelessCom.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.RegisterDependencies();

        #if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        #endif

        var app = builder.Build();

        using var scope = app.Services.CreateScope();

        // Migrate latest database changes during startup
        var dbContext = scope.ServiceProvider.GetRequiredService<ClimateDbContext>();
        dbContext.Database.Migrate();

        // Required to initialize the notify event handlers on room sensor connection events.
        _ = scope.ServiceProvider.GetRequiredService<IBleRoomSensorService>();
        
        return app;
    }
}