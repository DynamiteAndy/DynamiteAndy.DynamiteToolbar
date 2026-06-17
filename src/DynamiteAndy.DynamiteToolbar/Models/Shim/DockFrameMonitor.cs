using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct DockFrameMonitor
{
    public IntPtr MonitorPtr;
    public DockFrameMonitorLabel Streaming;
    public DockFrameMonitorLabel Recording;
    public DockFrameMonitorLabel CurrentScene;
    public DockFrameMonitorLabel Cpu;
    public DockFrameMonitorLabel Fps;
    public DockFrameMonitorLabel DroppedFrames;
    public DockFrameMonitorLabel Kbps;
}