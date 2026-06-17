using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Services.IO;

namespace DynamiteAndy.DynamiteToolbar.Services.Settings;

public class ObsSettingsService(
    IObsDataApi obsData,
    ISettingsPathResolver pathResolver,
    IFileService fileService) : ISettingsService
{
    public PluginSettings Load()
    {
        var settings = new PluginSettings();
        var path = pathResolver.GetSettingsPath();

        if (path == null || !fileService.FileExists(path))
        {
            return settings;
        }

        var data = obsData.LoadFromJson(path);
        if (data == IntPtr.Zero)
        {
            return settings;
        }

        try
        {
            settings.HideFloat = obsData.GetBool(data, nameof(settings.HideFloat));
            settings.HideStudioMode = obsData.GetBool(data, nameof(settings.HideStudioMode));
            settings.HideVirtualCamera = obsData.GetBool(data, nameof(settings.HideVirtualCamera));
            settings.HideReplayBuffer = obsData.GetBool(data, nameof(settings.HideReplayBuffer));
            settings.HideStartRecording = obsData.GetBool(data, nameof(settings.HideStartRecording));
            settings.HideStartStreaming = obsData.GetBool(data, nameof(settings.HideStartStreaming));

            settings.HideStreamingLabel = obsData.GetBool(data, nameof(settings.HideStreamingLabel));
            settings.HideRecordingLabel = obsData.GetBool(data, nameof(settings.HideRecordingLabel));
            settings.HideCurrentSceneLabel = obsData.GetBool(data, nameof(settings.HideCurrentSceneLabel));
            settings.HideCpuLabel = obsData.GetBool(data, nameof(settings.HideCpuLabel));
            settings.HideFpsLabel = obsData.GetBool(data, nameof(settings.HideFpsLabel));
            settings.HideDroppedFramesLabel = obsData.GetBool(data, nameof(settings.HideDroppedFramesLabel));
            settings.HideKbpsLabel = obsData.GetBool(data, nameof(settings.HideKbpsLabel));

            settings.HideStopRecordingModal = obsData.GetBool(data, nameof(settings.HideStopRecordingModal));
        }
        finally
        {
            obsData.Release(data);
        }

        return settings;
    }

    public void Save(PluginSettings settings)
    {
        var path = pathResolver.GetSettingsPath();
        if (path == null)
        {
            return;
        }

        var data = obsData.Create();

        try
        {
            obsData.SetBool(data, nameof(settings.HideFloat), settings.HideFloat);
            obsData.SetBool(data, nameof(settings.HideStudioMode), settings.HideStudioMode);
            obsData.SetBool(data, nameof(settings.HideVirtualCamera), settings.HideVirtualCamera);
            obsData.SetBool(data, nameof(settings.HideReplayBuffer), settings.HideReplayBuffer);
            obsData.SetBool(data, nameof(settings.HideStartRecording), settings.HideStartRecording);
            obsData.SetBool(data, nameof(settings.HideStartStreaming), settings.HideStartStreaming);

            obsData.SetBool(data, nameof(settings.HideStreamingLabel), settings.HideStreamingLabel);
            obsData.SetBool(data, nameof(settings.HideRecordingLabel), settings.HideRecordingLabel);
            obsData.SetBool(data, nameof(settings.HideCurrentSceneLabel), settings.HideCurrentSceneLabel);
            obsData.SetBool(data, nameof(settings.HideCpuLabel), settings.HideCpuLabel);
            obsData.SetBool(data, nameof(settings.HideFpsLabel), settings.HideFpsLabel);
            obsData.SetBool(data, nameof(settings.HideDroppedFramesLabel), settings.HideDroppedFramesLabel);
            obsData.SetBool(data, nameof(settings.HideKbpsLabel), settings.HideKbpsLabel);

            obsData.SetBool(data, nameof(settings.HideStopRecordingModal), settings.HideStopRecordingModal);

            obsData.SaveToJson(data, path);
        }
        finally
        {
            obsData.Release(data);
        }
    }
}