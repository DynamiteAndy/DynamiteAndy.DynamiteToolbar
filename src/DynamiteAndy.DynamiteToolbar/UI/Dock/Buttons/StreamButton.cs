using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;

public class StreamButton : QAbstractButton
{
    private readonly ILocalizationService _localizationService;
    private readonly IObsFrontendApi _obsFrontendApi;
    private readonly IQMessageBoxShim _qMessageBoxShim;
    private readonly IPubSubService _pubSubService;

    public StreamButton(
        IQtControlContext qtControlContext,
        ILocalizationService localizationService,
        IObsFrontendApi obsFrontendApi,
        IQMessageBoxShim qMessageBoxShim,
        IPubSubService pubSubService,
        IQAbstractButtonShim qAbstractButtonShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Buttons.StreamBtnPtr, qAbstractButtonShim, qWidgetShim)
    {
        _localizationService = localizationService;
        _obsFrontendApi = obsFrontendApi;
        _qMessageBoxShim = qMessageBoxShim;
        _pubSubService = pubSubService;

        var isActive = _obsFrontendApi.StreamingActive;

        Checked = isActive;
        Text = GetText(isActive);

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    protected override void OnClicked(bool isChecked)
    {
        if (_obsFrontendApi.StreamingActive)
        {
            _obsFrontendApi.StreamingStop();
        }
        else
        {
            _obsFrontendApi.StreamingStart();
        }
    }

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        var isActive = e.EventType switch
        {
            obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED or
            obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTING or
            obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPING => true,
            obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED => false,
            _ => Checked
        };

        if (isActive != Checked)
        {
            Checked = isActive;
            Text = GetText(isActive);
        }
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent s) => Visible = !s.Settings.HideStartStreaming;

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

    private string GetText(bool active)
        => _localizationService.T(active ? "Toolbar.Button.Stream.Stop" : "Toolbar.Button.Stream.Start");
}