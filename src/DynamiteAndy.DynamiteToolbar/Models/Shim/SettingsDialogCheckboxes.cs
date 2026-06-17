using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct SettingsDialogCheckboxes
{
    public IntPtr ShowHideFloatChkPtr;
    public IntPtr ShowHideStudioModeChkPtr;
    public IntPtr ShowHideVirtualCameraChkPtr;
    public IntPtr ShowHideReplayBufferChkPtr;
    public IntPtr ShowHideStartRecordingChkPtr;
    public IntPtr ShowHideStartStreamingChkPtr;
    public IntPtr ShowHideStreamingChkPtr;
    public IntPtr ShowHideRecordingChkPtr;
    public IntPtr ShowHideCurrentSceneChkPtr;
    public IntPtr ShowHideCPUChkPtr;
    public IntPtr ShowHideFPSChkPtr;
    public IntPtr ShowHideDroppedFramesChkPtr;
    public IntPtr ShowHideKbpsChkPtr;
    public IntPtr ShowHideStopRecordingChkPtr;
}