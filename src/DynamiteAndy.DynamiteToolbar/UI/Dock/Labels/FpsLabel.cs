using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;

public class FpsLabel : LabelGroup
{
    private readonly IObsApi _obsApi;
    private readonly ILocalizationService _localizationService;
    private readonly IPubSubService _pubSubService;

    public FpsLabel(
        IQtControlContext qtControlContext,
        IObsApi obsApi,
        ILocalizationService localizationService,
        IPubSubService pubSubService,
        IQLabelShim qLabelShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Monitor.Fps, qLabelShim, qWidgetShim)
    {
        _obsApi = obsApi;
        _localizationService = localizationService;
        _pubSubService = pubSubService;

        Name.Text = _localizationService.T("Toolbar.Label.FPS");

        _pubSubService.Subscribe<HeartbeatEvent>(OnHeartbeatEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnHeartbeatEvent(HeartbeatEvent e)
    {
        if (!Visible)
        {
            return;
        }

        var ovi = new obs_video_info();
        var success = _obsApi.GetVideoInfo(ref ovi) != 0;
        var target = success ? (double)ovi.fps_num / ovi.fps_den : 60.0;
        var current = _obsApi.GetActiveFps();

        Value.Text = $"{current:F2} / {target:F2}";
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e) => Visible = !e.Settings.HideFpsLabel;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _pubSubService.Unsubscribe<HeartbeatEvent>(OnHeartbeatEvent);
        _pubSubService.Unsubscribe<SettingsChangedEvent>(OnSettingsChangedEvent);

        base.Dispose(disposing);
    }
}