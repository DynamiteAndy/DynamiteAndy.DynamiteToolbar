using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct SettingsDialogGroupBoxes
{
    public IntPtr ShowHideButtonsGrpPtr;
    public IntPtr ShowHideLabelsGrpPtr;
    public IntPtr ShowHideConfirmationModalsGrpPtr;
}