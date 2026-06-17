namespace DynamiteAndy.DynamiteToolbar.Models;

public class PluginSettings
{
    public bool HideFloat { get; set; }
    public bool HideStudioMode { get; set; }
    public bool HideVirtualCamera { get; set; }
    public bool HideReplayBuffer { get; set; }
    public bool HideStartRecording { get; set; }
    public bool HideStartStreaming { get; set; }

    public bool HideStreamingLabel { get; set; }
    public bool HideRecordingLabel { get; set; }
    public bool HideCurrentSceneLabel { get; set; }
    public bool HideCpuLabel { get; set; }
    public bool HideFpsLabel { get; set; }
    public bool HideDroppedFramesLabel { get; set; }
    public bool HideKbpsLabel { get; set; }

    public bool HideStopRecordingModal { get; set; }


    public static PluginSettings FromShim(Shim.PluginSettings s) => new()
    {
        HideFloat = s.HideFloat == 1,
        HideStudioMode = s.HideStudioMode == 1,
        HideVirtualCamera = s.HideVirtualCamera == 1,
        HideReplayBuffer = s.HideReplayBuffer == 1,
        HideStartRecording = s.HideStartRecording == 1,
        HideStartStreaming = s.HideStartStreaming == 1,

        HideStreamingLabel = s.HideStreamingLabel == 1,
        HideRecordingLabel = s.HideRecordingLabel == 1,
        HideCurrentSceneLabel = s.HideCurrentSceneLabel == 1,
        HideCpuLabel = s.HideCpuLabel == 1,
        HideFpsLabel = s.HideFpsLabel == 1,
        HideDroppedFramesLabel = s.HideDroppedFramesLabel == 1,
        HideKbpsLabel = s.HideKbpsLabel == 1,

        HideStopRecordingModal = s.HideStopRecordingModal == 1,
    };

    public Shim.PluginSettings ToShim() => new()
    {
        HideFloat = HideFloat ? 1 : 0,
        HideStudioMode = HideStudioMode ? 1 : 0,
        HideVirtualCamera = HideVirtualCamera ? 1 : 0,
        HideReplayBuffer = HideReplayBuffer ? 1 : 0,
        HideStartRecording = HideStartRecording ? 1 : 0,
        HideStartStreaming = HideStartStreaming ? 1 : 0,

        HideStreamingLabel = HideStreamingLabel ? 1 : 0,
        HideRecordingLabel = HideRecordingLabel ? 1 : 0,
        HideCurrentSceneLabel = HideCurrentSceneLabel ? 1 : 0,
        HideCpuLabel = HideCpuLabel ? 1 : 0,
        HideFpsLabel = HideFpsLabel ? 1 : 0,
        HideDroppedFramesLabel = HideDroppedFramesLabel ? 1 : 0,
        HideKbpsLabel = HideKbpsLabel ? 1 : 0,

        HideStopRecordingModal = HideStopRecordingModal ? 1 : 0,
    };
}