using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.IO;
using DynamiteAndy.DynamiteToolbar.Services.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace DynamiteAndy.DynamiteToolbar.DependencyInjection;

public static class ObsServiceExtensions
{
    public static void AddDynamiteToolbarServices(this IServiceCollection services)
    {
        services.AddSingleton<IObsModuleContext, ObsModuleContext>();
        services.AddSingleton<IObsFrontendApi, ObsFrontendApi>();
        services.AddSingleton<IObsApi, ObsApi>();
        services.AddSingleton<IObsDataApi, ObsDataApi>();
        services.AddSingleton<IObsPlatformApi, ObsPlatformApi>();
        services.AddSingleton<IObsTextLookupApi, ObsTextLookupApi>();
        services.AddSingleton<ILogger, ObsLogger>();
        services.AddSingleton<ILocalizationService, ObsLocalizationService>();

        services.AddSingleton<ISettingsPathResolver>(sp =>
            new ObsSettingsPathResolver(
                "dynamiteandy.dynamite-toolbar.settings.json",
                sp.GetRequiredService<IObsApi>(),
                sp.GetRequiredService<IFileService>(),
                sp.GetRequiredService<ILogger>()));

        services.AddSingleton<ISettingsService, ObsSettingsService>();
    }
}