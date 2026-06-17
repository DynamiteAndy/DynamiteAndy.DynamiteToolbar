using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct DockFrameMonitorLabel
{
    public IntPtr ContainerHLayoutPtr;
    public IntPtr NameLblPtr;
    public IntPtr DelimiterLblPtr;
    public IntPtr ValueLblPtr;
    public IntPtr SeparatorLblPtr;
}