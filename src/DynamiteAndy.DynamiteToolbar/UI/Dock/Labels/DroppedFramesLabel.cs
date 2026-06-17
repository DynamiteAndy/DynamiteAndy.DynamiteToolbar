using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;

public class DroppedFramesLabel : LabelGroup
{
    private readonly IObsApi _obsApi;
    private readonly IObsFrontendApi _obsFrontendApi;
    private readonly ILocalizationService _localizationService;
    private readonly IPubSubService _pubSubService;

    private bool _visiblityFromSetting = true;
    private bool _visibilityFromStreaming;

    public DroppedFramesLabel(
        IQtControlContext qtControlContext,
        IObsApi obsApi,
        IObsFrontendApi obsFrontendApi,
        ILocalizationService localizationService,
        IPubSubService pubSubService,
        IQLabelShim qLabelShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Monitor.DroppedFrames, qLabelShim, qWidgetShim)
    {
        _obsApi = obsApi;
        _obsFrontendApi = obsFrontendApi;
        _localizationService = localizationService;
        _pubSubService = pubSubService;

        _visibilityFromStreaming = _obsFrontendApi.StreamingActive;

        Name.Text = _localizationService.T("Toolbar.Label.DroppedFrames");

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<HeartbeatEvent>(OnHeartbeatEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        switch (e.EventType)
        {
            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTING:
            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED:
            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPING:
                _visibilityFromStreaming = true;

                Visible = _visiblityFromSetting && _visibilityFromStreaming;
                break;

            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED:
                _visibilityFromStreaming = false;

                Visible = _visiblityFromSetting && _visibilityFromStreaming;
                Value.Text = "0 (0.0%)";
                break;
        }
    }

    public void OnHeartbeatEvent(HeartbeatEvent e)
    {
        if (!Visible)
        {
            return;
        }

        var streamOutput = _obsFrontendApi.GetStreamingOutput();

        if (streamOutput != IntPtr.Zero)
        {
            try
            {
                var dropped = _obsApi.OutputGetFramesDropped(streamOutput);
                var total = _obsApi.OutputGetTotalFrames(streamOutput);
                var dropPercent = (total > 0) ? ((double)dropped / total * 100.0) : 0.0;

                Value.Text = $"{dropped} ({dropPercent:F1}%)";
            }
            finally
            {
                _obsApi.OutputRelease(streamOutput);
            }
        }
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e)
    {
        _visiblityFromSetting = !e.Settings.HideDroppedFramesLabel;

        Visible = _visiblityFromSetting && _visibilityFromStreaming;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _pubSubService.Unsubscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Unsubscribe<HeartbeatEvent>(OnHeartbeatEvent);
        _pubSubService.Unsubscribe<SettingsChangedEvent>(OnSettingsChangedEvent);

        base.Dispose(disposing);
    }
}