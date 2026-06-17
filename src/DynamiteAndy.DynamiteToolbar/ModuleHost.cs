using DynamiteAndy.DynamiteToolbar.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DynamiteAndy.DynamiteToolbar;

public static class ModuleHost
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("ModuleHost has not been initialized.");

    public static void Initialize()
    {
        if (_serviceProvider != null)
        {
            return;
        }

        var services = new ServiceCollection();

        services.AddCoreServices();
        services.AddNativeServices();
        services.AddDynamiteToolbarServices();
        services.AddBridgeServices();
        services.AddUIServices();
        services.AddPluginServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public static void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _serviceProvider = null;
    }
}