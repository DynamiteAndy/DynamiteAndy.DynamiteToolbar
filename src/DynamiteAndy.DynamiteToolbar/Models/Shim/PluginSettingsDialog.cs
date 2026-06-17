using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct PluginSettingsDialog
{
    public IntPtr PluginSettingsDialogPtr;
    public SettingsDialogGroupBoxes GroupBoxes;
    public SettingsDialogLabels Labels;
    public SettingsDialogCheckboxes Checkboxes;
}