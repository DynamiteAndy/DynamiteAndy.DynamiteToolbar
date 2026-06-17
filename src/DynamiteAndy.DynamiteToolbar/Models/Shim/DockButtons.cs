using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct DockButtons
{
    public IntPtr FloatBtnPtr;
    public IntPtr StudioModeBtnPtr;
    public IntPtr VirtualCameraBtnPtr;
    public IntPtr ReplayBufferBtnPtr;
    public IntPtr RecordBtnPtr;
    public IntPtr RecordPausedBtnPtr;
    public IntPtr StreamBtnPtr;
}