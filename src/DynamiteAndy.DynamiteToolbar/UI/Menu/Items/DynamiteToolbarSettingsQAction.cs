using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.UI.Dialog;

namespace DynamiteAndy.DynamiteToolbar.UI.Menu.Items;

public class DynamiteToolbarSettingsQAction(
    IntPtr ptr,
    IQActionShim shim,
    SettingsDialog settingsDialog) : QAction(ptr, shim)
{
    protected override void OnTriggered(bool isChecked) => settingsDialog.Show();
}