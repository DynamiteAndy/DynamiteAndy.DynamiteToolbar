using DynamiteAndy.DynamiteToolbar.Interop.Bridge;
using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using Microsoft.Extensions.DependencyInjection;

namespace DynamiteAndy.DynamiteToolbar.DependencyInjection;

public static class BridgeServiceExtensions
{
    public static void AddBridgeServices(this IServiceCollection services)
    {
        AddQtServices(services);
        AddServices(services);
    }

    private static void AddQtServices(IServiceCollection services)
    {
        services.AddSingleton<IQWidgetShim, QWidgetShim>();
        services.AddSingleton<IQActionShim, QActionShim>();
        services.AddSingleton<IQLabelShim, QLabelShim>();
        services.AddSingleton<IQGroupBoxShim, QGroupBoxShim>();
        services.AddSingleton<IQAbstractButtonShim, QAbstractButtonShim>();
        services.AddSingleton<IQDockWidgetShim, QDockWidgetShim>();
        services.AddSingleton<IQMessageBoxShim, QMessageBoxShim>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<IDockShim, DockShim>();
        services.AddSingleton<ISettingsDialogShim, SettingsDialogShim>();
    }
}