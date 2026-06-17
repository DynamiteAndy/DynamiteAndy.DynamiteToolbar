using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Services.IO;

namespace DynamiteAndy.DynamiteToolbar.Services.Settings;

public sealed class ObsSettingsPathResolver(
    string fileName,
    IObsApi obsApi,
    IFileService fileService,
    ILogger logger) : ISettingsPathResolver
{
    public string? GetSettingsPath()
    {
        try
        {
            var dir = obsApi.ModuleGetConfigPath(fileName);
            var directory = Path.GetDirectoryName(dir);

            if (!string.IsNullOrEmpty(directory))
            {
                fileService.CreateDirectory(directory);
            }

            return dir;
        }
        catch (Exception ex)
        {
            logger.Error($"SettingsPathResolver: Exception while resolving path: {ex.Message}");
            return null;
        }
    }
}