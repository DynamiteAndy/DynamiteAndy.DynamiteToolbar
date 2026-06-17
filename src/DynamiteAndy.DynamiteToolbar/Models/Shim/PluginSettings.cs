using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Shim;

[StructLayout(LayoutKind.Sequential)]
public struct PluginSettings
{
    // Buttons
    public int HideFloat;
    public int HideStudioMode;
    public int HideVirtualCamera;
    public int HideReplayBuffer;
    public int HideStartRecording;
    public int HideStartStreaming;

    // Labels
    public int HideStreamingLabel;
    public int HideRecordingLabel;
    public int HideCurrentSceneLabel;
    public int HideCpuLabel;
    public int HideFpsLabel;
    public int HideDroppedFramesLabel;
    public int HideKbpsLabel;

    // Confirmation modals
    public int HideStopRecordingModal;
}