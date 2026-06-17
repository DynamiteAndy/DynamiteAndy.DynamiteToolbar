using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;

public class StudioModeButton : QAbstractButton
{
    private readonly IObsFrontendApi _obsFrontendApi;
    private readonly IPubSubService _pubSubService;

    public StudioModeButton(
        IQtControlContext qtControlContext,
        ILocalizationService localizationService,
        IObsFrontendApi obsFrontendApi,
        IPubSubService pubSubService,
        IQAbstractButtonShim qAbstractButtonShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Buttons.StudioModeBtnPtr, qAbstractButtonShim, qWidgetShim)
    {
        _obsFrontendApi = obsFrontendApi;
        _pubSubService = pubSubService;

        Checked = _obsFrontendApi.PreviewProgramModeActive;
        Text = localizationService.T("Toolbar.Button.StudioMode");

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    protected override void OnClicked(bool isChecked) => _obsFrontendApi.SetPreviewProgramMode(!_obsFrontendApi.PreviewProgramModeActive);

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        Checked = e.EventType switch
        {
            obs_frontend_event.OBS_FRONTEND_EVENT_STUDIO_MODE_ENABLED => true,
            obs_frontend_event.OBS_FRONTEND_EVENT_STUDIO_MODE_DISABLED => false,
            _ => Checked
        };
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent s) => Visible = !s.Settings.HideStudioMode;

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
}