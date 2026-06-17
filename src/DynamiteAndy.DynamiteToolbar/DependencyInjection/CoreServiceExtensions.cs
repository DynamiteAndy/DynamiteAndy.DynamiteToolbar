using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.IO;
using Microsoft.Extensions.DependencyInjection;

namespace DynamiteAndy.DynamiteToolbar.DependencyInjection;

public static class CoreServiceExtensions
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IPubSubService, PubSubService>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IHeartbeatService, HeartbeatService>();
    }
}