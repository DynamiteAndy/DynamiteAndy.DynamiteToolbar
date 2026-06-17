using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;

public class KbpsLabel : LabelGroup
{
    private readonly IObsApi _obsApi;
    private readonly IObsFrontendApi _obsFrontendApi;
    private readonly IObsPlatformApi _obsPlatformApi;
    private readonly ILocalizationService _localizationService;
    private readonly IPubSubService _pubSubService;

    private ulong _lastBytesSent = 0;
    private ulong _lastBytesSentTime = 0;
    private bool _visiblityFromSetting = true;
    private bool _visibilityFromStreaming;

    private static TimeSpan TargetInterval => TimeSpan.FromSeconds(2);

    public KbpsLabel(
        IQtControlContext qtControlContext,
        IObsApi obsApi,
        IObsFrontendApi obsFrontendApi,
        IObsPlatformApi obsPlatformApi,
        ILocalizationService localizationService,
        IPubSubService pubSubService,
        IQLabelShim qLabelShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Monitor.Kbps, qLabelShim, qWidgetShim)
    {
        _obsApi = obsApi;
        _obsFrontendApi = obsFrontendApi;
        _obsPlatformApi = obsPlatformApi;
        _localizationService = localizationService;
        _pubSubService = pubSubService;

        _visibilityFromStreaming = _obsFrontendApi.StreamingActive;

        Name.Text = _localizationService.T("Toolbar.Label.Kbps");

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
                Value.Text = "0";

                _lastBytesSent = 0;
                _lastBytesSentTime = 0;
                break;
        }
    }

    public void OnHeartbeatEvent(HeartbeatEvent e)
    {
        if (!Visible)
        {
            return;
        }

        var currentTimeNs = _obsPlatformApi.OSGetTimeNs();

        if (_lastBytesSentTime != 0)
        {
            var elapsedTicks = (long)(currentTimeNs - _lastBytesSentTime) / 100;
            var actualTimePassed = TimeSpan.FromTicks(elapsedTicks);

            if (actualTimePassed < TargetInterval)
            {
                return;
            }
        }

        var streamOutput = _obsFrontendApi.GetStreamingOutput();

        if (streamOutput != IntPtr.Zero)
        {
            try
            {
                var bytesSent = _obsApi.OutputGetTotalBytes(streamOutput);
                var bytesSentTime = _obsPlatformApi.OSGetTimeNs();

                if (_lastBytesSentTime == 0)
                {
                    _lastBytesSent = bytesSent;
                    _lastBytesSentTime = bytesSentTime;

                    Value.Text = "0";

                    return;
                }

                if (bytesSent < _lastBytesSent)
                {
                    bytesSent = 0;
                }

                if (bytesSent == 0)
                {
                    _lastBytesSent = 0;
                }

                var bitsBetween = (bytesSent - _lastBytesSent) * 8;
                var timePassed = (bytesSentTime - _lastBytesSentTime) / 1000000000.0;
                var kbitsPerSec = 0.0;

                if (timePassed > 0)
                {
                    kbitsPerSec = bitsBetween / timePassed / 1000.0;
                }

                Value.Text = $"{Math.Round(kbitsPerSec):F0}";

                _lastBytesSent = bytesSent;
                _lastBytesSentTime = bytesSentTime;
            }
            finally
            {
                _obsApi.OutputRelease(streamOutput);
            }
        }
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e)
    {
        _visiblityFromSetting = !e.Settings.HideKbpsLabel;

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