using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct PluginDock
{
    public IntPtr DockPtr;
    public DockFrameMonitor Monitor;
    public DockButtons Buttons;
    public DockSpacers Spacers;
}