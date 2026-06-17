using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;

public class StreamingLabel : LabelGroup
{
    private readonly ILocalizationService _localizationService;
    private readonly IPubSubService _pubSubService;
    private readonly TimeProvider _timeProvider;

    private DateTimeOffset? _startTime;

    public StreamingLabel(
        IQtControlContext qtControlContext,
        ILocalizationService localizationService,
        IPubSubService pubSubService,
        TimeProvider timeProvider,
        IQLabelShim qLabelShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Monitor.Streaming, qLabelShim, qWidgetShim)
    {
        _localizationService = localizationService;
        _pubSubService = pubSubService;
        _timeProvider = timeProvider;

        Name.Text = _localizationService.T("Toolbar.Label.Streaming");
        Value.Text = _localizationService.T("Toolbar.Status.Inactive");

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<HeartbeatEvent>(OnHeartbeatEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        switch (e.EventType)
        {
            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTING:
                _startTime = null;

                Value.Text = _localizationService.T("Toolbar.Status.Starting");
                break;

            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED:
                _startTime = _timeProvider.GetUtcNow();

                Value.Text = FormatElapsed(TimeSpan.Zero);
                break;

            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPING:
                Value.Text = _localizationService.T("Toolbar.Status.Stopping");
                break;

            case obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED:
                _startTime = null;

                Value.Text = _localizationService.T("Toolbar.Status.Inactive");
                break;
        }
    }

    public void OnHeartbeatEvent(HeartbeatEvent _)
    {
        if (!Visible)
        {
            return;
        }

        if (_startTime is { } start)
        {
            Value.Text = FormatElapsed(_timeProvider.GetUtcNow() - start);
        }
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e) => Visible = !e.Settings.HideStreamingLabel;

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

    private static string FormatElapsed(TimeSpan ts) => ts.ToString(@"hh\:mm\:ss");
}