using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;

public class ReplayBufferButton : QAbstractButton
{
    private readonly ILocalizationService _localizationService;
    private readonly IObsFrontendApi _obsFrontendApi;
    private readonly IPubSubService _pubSubService;

    public ReplayBufferButton(
        IQtControlContext qtControlContext,
        ILocalizationService localizationService,
        IObsFrontendApi obsFrontendApi,
        IPubSubService pubSubService,
        IQAbstractButtonShim qAbstractButtonShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Buttons.ReplayBufferBtnPtr, qAbstractButtonShim, qWidgetShim)
    {
        _localizationService = localizationService;
        _obsFrontendApi = obsFrontendApi;
        _pubSubService = pubSubService;

        var isActive = _obsFrontendApi.ReplayBufferActive;

        Checked = isActive;
        Tooltip = GetLabel(isActive);

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    protected override void OnClicked(bool isChecked)
    {
        if (_obsFrontendApi.ReplayBufferActive)
        {
            _obsFrontendApi.ReplayBufferSave();
            _obsFrontendApi.ReplayBufferStop();
        }
        else
        {
            _obsFrontendApi.ReplayBufferStart();
        }
    }

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        var isActive = e.EventType switch
        {
            obs_frontend_event.OBS_FRONTEND_EVENT_REPLAY_BUFFER_STARTED => true,
            obs_frontend_event.OBS_FRONTEND_EVENT_REPLAY_BUFFER_STOPPED => false,
            _ => Checked
        };

        if (isActive != Checked)
        {
            Checked = isActive;
            Tooltip = GetLabel(isActive);
        }
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent s) => Visible = !s.Settings.HideReplayBuffer;

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

    private string GetLabel(bool active)
        => _localizationService.T(active ? "Toolbar.Button.ReplayBuffer.Stop" : "Toolbar.Button.ReplayBuffer.Start");
}