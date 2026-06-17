using DynamiteAndy.DynamiteToolbar.Interop.Bridge;
using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.Settings;

namespace DynamiteAndy.DynamiteToolbar.UI.Dialog;

public sealed class SettingsDialog(
    ISettingsDialogShim settingsDialogShim,
    ISettingsService settingsService,
    ILocalizationService localizationService,
    IQGroupBoxShim qGroupBoxShim,
    IQAbstractButtonShim qAbstractButtonShim,
    IQLabelShim qLabelShim,
    IPubSubService pubSubService)
{
    public void Show()
    {
        var currentSettings = settingsService.Load().ToShim();
        var dialog = settingsDialogShim.CreateSettingsDialog(currentSettings);

        if (dialog.PluginSettingsDialogPtr == IntPtr.Zero)
        {
            return;
        }

        SetGroupBoxTitle(dialog.GroupBoxes.ShowHideButtonsGrpPtr, "Toolbar.Settings.ShowHideButtons.Title");
        SetSettingText(dialog.Checkboxes.ShowHideFloatChkPtr, "Toolbar.Button.Float.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideStudioModeChkPtr, "Toolbar.Button.StudioMode.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideVirtualCameraChkPtr, "Toolbar.Button.VirtualCamera.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideReplayBufferChkPtr, "Toolbar.Button.ReplayBuffer.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideStartRecordingChkPtr, "Toolbar.Button.Record.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideStartStreamingChkPtr, "Toolbar.Button.Stream.Setting");

        SetGroupBoxTitle(dialog.GroupBoxes.ShowHideLabelsGrpPtr, "Toolbar.Settings.ShowHideLabels.Title");
        SetSettingText(dialog.Checkboxes.ShowHideStreamingChkPtr, "Toolbar.Label.Streaming.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideRecordingChkPtr, "Toolbar.Label.Recording.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideCurrentSceneChkPtr, "Toolbar.Label.CurrentScene.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideCPUChkPtr, "Toolbar.Label.CPU.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideFPSChkPtr, "Toolbar.Label.FPS.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideDroppedFramesChkPtr, "Toolbar.Label.DroppedFrames.Setting");
        SetSettingText(dialog.Checkboxes.ShowHideKbpsChkPtr, "Toolbar.Label.Kbps.Setting");

        SetGroupBoxTitle(dialog.GroupBoxes.ShowHideConfirmationModalsGrpPtr, "Toolbar.Settings.ShowHideConfirmationModals.Title");
        SetSettingText(dialog.Checkboxes.ShowHideStopRecordingChkPtr, "ConfirmationModal.Recording.Setting");

        SetLabelText(dialog.Labels.CreditsLblPtr, "Toolbar.Settings.Credits");
        SetLabelText(dialog.Labels.ContributorsLblPtr, "Toolbar.Settings.Contributors");

        try
        {
            if (settingsDialogShim.ShowSettingsDialog(dialog.PluginSettingsDialogPtr, ref currentSettings) == 1)
            {
                var updatedSettings = PluginSettings.FromShim(currentSettings);

                settingsService.Save(updatedSettings);
                pubSubService.Raise(new SettingsChangedEvent(updatedSettings));
            }
        }
        finally
        {
            settingsDialogShim.DestroySettingsDialog(dialog.PluginSettingsDialogPtr);
        }
    }

    private void SetGroupBoxTitle(nint ptr, string key) => qGroupBoxShim.SetTitle(ptr, localizationService.T(key));

    private void SetSettingText(nint ptr, string key) => qAbstractButtonShim.SetText(ptr, localizationService.T(key));

    private void SetLabelText(nint ptr, string key) => qLabelShim.SetText(ptr, localizationService.T(key));
}