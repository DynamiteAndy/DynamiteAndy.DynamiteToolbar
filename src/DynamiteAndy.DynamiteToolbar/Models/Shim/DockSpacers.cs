using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct DockSpacers
{
    public IntPtr FloatSpacerPtr;
    public IntPtr StudioModeSpacerPtr;
    public IntPtr StatsSpacerPtr;
    public IntPtr VirtualCameraSpacerPtr;
    public IntPtr ReplayBufferSpacerPtr;
    public IntPtr RecordSpacerPtr;
}