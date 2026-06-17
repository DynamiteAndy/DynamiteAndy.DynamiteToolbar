using DynamiteAndy.DynamiteToolbar.Models.Shim;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge;

public sealed partial class SettingsDialogShim : ISettingsDialogShim
{
    [LibraryImport(Constants.LibraryName)]
    private static partial PluginSettingsDialog create_settings_dialog(ref PluginSettings current);

    [LibraryImport(Constants.LibraryName)]
    private static partial void destroy_settings_dialog(IntPtr dlg);

    [LibraryImport(Constants.LibraryName)]
    private static partial int show_settings_dialog(IntPtr dlg, ref PluginSettings result);

    public PluginSettingsDialog CreateSettingsDialog(PluginSettings current) => create_settings_dialog(ref current);

    public void DestroySettingsDialog(IntPtr dlg)
    {
        if (dlg != IntPtr.Zero)
        {
            destroy_settings_dialog(dlg);
        }
    }

    public int ShowSettingsDialog(IntPtr dlg, ref PluginSettings result) => show_settings_dialog(dlg, ref result);
}