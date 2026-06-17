using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct SettingsDialogLabels
{
    public IntPtr CreditsLblPtr;
    public IntPtr ContributorsLblPtr;
    public IntPtr VersionValueLblPtr;
}