using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;

public class RecordPausedButton : QAbstractButton
{
    private bool _visibilityFromSetting = true;
    private bool _visibilityFromRecording;

    private readonly IPubSubService _pubSubService;

    public RecordPausedButton(
        IQtControlContext qtControlContext,
        ILocalizationService localizationService,
        IObsFrontendApi obsFrontendApi,
        IPubSubService pubSubService,
        IQAbstractButtonShim qAbstractButtonShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Buttons.RecordPausedBtnPtr, qAbstractButtonShim, qWidgetShim)
    {
        _visibilityFromRecording = obsFrontendApi.RecordingPaused;
        _pubSubService = pubSubService;

        Checked = _visibilityFromRecording;
        Tooltip = localizationService.T("Toolbar.Button.Record.Paused");

        UpdateVisibility();

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        var isActive = e.EventType switch
        {
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED => true,
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED or
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_UNPAUSED or
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPING or
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPED => false,
            _ => Checked
        };

        if (isActive != Checked)
        {
            _visibilityFromRecording = isActive;

            Checked = isActive;

            UpdateVisibility();
        }
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e)
    {
        _visibilityFromSetting = !e.Settings.HideStartRecording;

        UpdateVisibility();
    }

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

    private void UpdateVisibility() => Visible = _visibilityFromSetting && _visibilityFromRecording;
}