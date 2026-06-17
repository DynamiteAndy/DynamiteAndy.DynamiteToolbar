using DynamiteAndy.DynamiteToolbar.Models;

namespace DynamiteAndy.DynamiteToolbar.Services.Settings;

public interface ISettingsService
{
    void Save(PluginSettings settings);
    PluginSettings Load();
}