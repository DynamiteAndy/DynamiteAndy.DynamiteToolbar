using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Obs;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public class ObsFrontendApi : IObsFrontendApi
{
    public bool StreamingActive => ObsInterop.ObsFrontendApi.obs_frontend_streaming_active() != 0;
    public bool RecordingActive => ObsInterop.ObsFrontendApi.obs_frontend_recording_active() != 0;
    public bool RecordingPaused => ObsInterop.ObsFrontendApi.obs_frontend_recording_paused() != 0;
    public bool ReplayBufferActive => ObsInterop.ObsFrontendApi.obs_frontend_replay_buffer_active() != 0;
    public bool VirtualcamActive => ObsInterop.ObsFrontendApi.obs_frontend_virtualcam_active() != 0;
    public bool PreviewProgramModeActive => ObsInterop.ObsFrontendApi.obs_frontend_preview_program_mode_active() != 0;

    public void AddEventCallback(IntPtr callback, IntPtr privateData)
    {
        unsafe
        {
            ObsInterop.ObsFrontendApi.obs_frontend_add_event_callback(
                (delegate* unmanaged[Cdecl]<obs_frontend_event, void*, void>)callback,
                (void*)privateData
            );
        }
    }

    public void RemoveEventCallback(IntPtr callback, IntPtr privateData)
    {
        unsafe
        {
            ObsInterop.ObsFrontendApi.obs_frontend_remove_event_callback(
                (delegate* unmanaged[Cdecl]<obs_frontend_event, void*, void>)callback,
                (void*)privateData
            );
        }
    }

    public void SetPreviewProgramMode(bool enable)
        => ObsInterop.ObsFrontendApi.obs_frontend_set_preview_program_mode((byte)(enable ? 1 : 0));

    public void ReplayBufferStart() => ObsInterop.ObsFrontendApi.obs_frontend_replay_buffer_start();

    public void ReplayBufferSave() => ObsInterop.ObsFrontendApi.obs_frontend_replay_buffer_save();

    public void ReplayBufferStop() => ObsInterop.ObsFrontendApi.obs_frontend_replay_buffer_stop();

    public void StreamingStart() => ObsInterop.ObsFrontendApi.obs_frontend_streaming_start();

    public void StreamingStop() => ObsInterop.ObsFrontendApi.obs_frontend_streaming_stop();

    public void RecordingStart() => ObsInterop.ObsFrontendApi.obs_frontend_recording_start();

    public void RecordingPause(bool pause) => ObsInterop.ObsFrontendApi.obs_frontend_recording_pause((byte)(pause ? 1 : 0));

    public void RecordingStop() => ObsInterop.ObsFrontendApi.obs_frontend_recording_stop();

    public void StartVirtualcam() => ObsInterop.ObsFrontendApi.obs_frontend_start_virtualcam();

    public void StopVirtualcam() => ObsInterop.ObsFrontendApi.obs_frontend_stop_virtualcam();

    public unsafe IntPtr GetCurrentScene() => (IntPtr)ObsInterop.ObsFrontendApi.obs_frontend_get_current_scene();

    public unsafe IntPtr GetStreamingOutput() => (IntPtr)ObsInterop.ObsFrontendApi.obs_frontend_get_streaming_output();

    public nint AddToolsMenuQAction(string actionLabel)
        => ObsFrontendApiShim.add_tools_menu_qaction(actionLabel);
}