using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;

public class RecordButton : QAbstractButton
{
    private readonly ILocalizationService _localizationService;
    private readonly IObsFrontendApi _obsFrontendApi;
    private readonly IQMessageBoxShim _qMessageBoxShim;
    private readonly IPubSubService _pubSubService;

    private bool _visibilityFromSetting = true;
    private bool _confirmationModalVisibilityFromSetting = true;
    private bool _visibilityFromRecording;

    public RecordButton(
        IQtControlContext qtControlContext,
        ILocalizationService localizationService,
        IObsFrontendApi obsFrontendApi,
        IQMessageBoxShim qMessageBoxShim,
        IPubSubService pubSubService,
        IQAbstractButtonShim qAbstractButtonShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Buttons.RecordBtnPtr, qAbstractButtonShim, qWidgetShim)
    {
        _localizationService = localizationService;
        _obsFrontendApi = obsFrontendApi;
        _qMessageBoxShim = qMessageBoxShim;
        _pubSubService = pubSubService;

        var isPaused = _obsFrontendApi.RecordingPaused;
        var isActive = _obsFrontendApi.RecordingActive;

        _visibilityFromRecording = !isPaused;

        Checked = isActive || isPaused;
        Tooltip = GetText(isActive || isPaused);

        UpdateVisibility();

        _pubSubService.Subscribe<ObsFrontendEvent>(OnObsFrontendEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    protected override void OnClicked(bool isChecked)
    {
        var wasRecording = _obsFrontendApi.RecordingActive || _obsFrontendApi.RecordingPaused;

        if (wasRecording)
        {
            if (_confirmationModalVisibilityFromSetting)
            {
                _obsFrontendApi.RecordingPause(true);
            }

            var shouldStop = !_confirmationModalVisibilityFromSetting ||
                _qMessageBoxShim.ShowConfirmation(
                _localizationService.T("ConfirmationModal.Recording.StopTitle"),
                _localizationService.T("ConfirmationModal.Recording.StopText"));

            if (shouldStop)
            {
                _obsFrontendApi.RecordingStop();
            }
            else
            {
                _obsFrontendApi.RecordingPause(false);
            }
        }
        else
        {
            _obsFrontendApi.RecordingStart();
        }
    }

    public void OnObsFrontendEvent(ObsFrontendEvent e)
    {
        var (isActive, isVisible) = e.EventType switch
        {
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED or
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPING or
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_UNPAUSED => (true, true),
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_PAUSED => (true, false),
            obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STOPPED => (false, true),
            _ => (Checked, _visibilityFromRecording)
        };

        if (isActive != Checked)
        {
            Checked = isActive;
            Tooltip = GetText(isActive);
        }

        if (isVisible != _visibilityFromRecording)
        {
            _visibilityFromRecording = isVisible;

            UpdateVisibility();
        }
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e)
    {
        _visibilityFromSetting = !e.Settings.HideStartRecording;
        _confirmationModalVisibilityFromSetting = !e.Settings.HideStopRecordingModal;

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

    private string GetText(bool active) =>
        _localizationService.T(active ? "Toolbar.Button.Record.Stop" : "Toolbar.Button.Record.Start");
}