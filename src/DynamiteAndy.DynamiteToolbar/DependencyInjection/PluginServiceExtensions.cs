using DynamiteAndy.DynamiteToolbar.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DynamiteAndy.DynamiteToolbar.DependencyInjection;

public static class PluginServiceExtensions
{
    public static void AddPluginServices(this IServiceCollection services)
    {
        services.AddSingleton<IObsPlugin, Plugin>();
    }
}