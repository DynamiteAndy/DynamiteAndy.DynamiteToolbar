using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;

public class CurrentSceneLabel : LabelGroup
{
    private readonly IObsApi _obsApi;
    private readonly IObsFrontendApi _obsFrontendApi;
    private readonly ILocalizationService _localizationService;
    private readonly IPubSubService _pubSubService;

    public CurrentSceneLabel(
        IQtControlContext qtControlContext,
        IObsApi obsApi,
        IObsFrontendApi obsFrontendApi,
        ILocalizationService localizationService,
        IPubSubService pubSubService,
        IQLabelShim qLabelShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Monitor.CurrentScene, qLabelShim, qWidgetShim)
    {
        _obsApi = obsApi;
        _obsFrontendApi = obsFrontendApi;
        _localizationService = localizationService;
        _pubSubService = pubSubService;

        Name.Text = _localizationService.T("Toolbar.Label.CurrentScene");

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        if (e.EventType != obs_frontend_event.OBS_FRONTEND_EVENT_SCENE_CHANGED || !Visible)
        {
            return;
        }

        Value.Text = GetSceneName();
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e) => Visible = !e.Settings.HideCurrentSceneLabel;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _pubSubService.Unsubscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Unsubscribe<SettingsChangedEvent>(OnSettingsChangedEvent);

        base.Dispose(disposing);
    }

    private string GetSceneName()
    {
        var scene = _obsFrontendApi.GetCurrentScene();
        var name = "-";

        if (scene != IntPtr.Zero)
        {
            name = _obsApi.SourceGetName(scene) ?? "-";
            _obsApi.SourceRelease(scene);
        }

        return name;
    }
}