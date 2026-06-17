using DynamiteAndy.DynamiteToolbar.Interop.Native;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.IO;
using Microsoft.Extensions.DependencyInjection;

namespace DynamiteAndy.DynamiteToolbar.DependencyInjection;

public static class NativeServiceExtensions
{
    public static void AddNativeServices(this IServiceCollection services)
    {
        services.AddSingleton<IRuntimeInformationProxy, RuntimeInformationProxy>();
        services.AddSingleton<INativeLibraryProxy, NativeLibraryProxy>();

        services.AddSingleton<INativeLibraryLoader>(sp =>
            new NativeLibraryLoader(
                    "dynamiteandy.dynamite-toolbar.dock",
                    sp.GetRequiredService<ILogger>(),
                    sp.GetRequiredService<IFileService>(),
                    sp.GetRequiredService<IObsApi>(),
                    sp.GetRequiredService<IRuntimeInformationProxy>(),
                    sp.GetRequiredService<INativeLibraryProxy>()));
    }
}