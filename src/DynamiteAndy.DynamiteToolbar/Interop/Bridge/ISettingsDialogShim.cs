using DynamiteAndy.DynamiteToolbar.Models.Shim;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge;

public interface ISettingsDialogShim
{
    PluginSettingsDialog CreateSettingsDialog(PluginSettings current);
    void DestroySettingsDialog(IntPtr dlg);
    int ShowSettingsDialog(IntPtr dlg, ref PluginSettings result);
}