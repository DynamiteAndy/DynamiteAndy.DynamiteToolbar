namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public interface IObsFrontendApi
{
    bool StreamingActive { get; }
    bool RecordingActive { get; }
    bool RecordingPaused { get; }
    bool ReplayBufferActive { get; }
    bool VirtualcamActive { get; }
    bool PreviewProgramModeActive { get; }

    void AddEventCallback(IntPtr callback, IntPtr privateData);
    void RemoveEventCallback(IntPtr callback, IntPtr privateData);

    void SetPreviewProgramMode(bool enable);
    void ReplayBufferStart();
    void ReplayBufferSave();
    void ReplayBufferStop();
    void StreamingStart();
    void StreamingStop();
    void RecordingStart();
    void RecordingPause(bool pause);
    void RecordingStop();
    void StartVirtualcam();
    void StopVirtualcam();

    IntPtr GetCurrentScene();

    IntPtr GetStreamingOutput();

    IntPtr AddToolsMenuQAction(string actionLabel);
}