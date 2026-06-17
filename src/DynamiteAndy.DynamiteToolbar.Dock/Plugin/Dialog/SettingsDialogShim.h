#pragma once
#include "ModuleShim.h"

struct PluginSettings {
	// Buttons
	int hideFloat;
	int hideStudioMode;
	int hideVirtualCamera;
	int hideReplayBuffer;
	int hideStartRecording;
	int hideStartStreaming;

	// Labels
	int hideStreamingLabel;
	int hideRecordingLabel;
	int hideCurrentSceneLabel;
	int hideCpuLabel;
	int hideFpsLabel;
	int hideDroppedFramesLabel;
	int hideKbpsLabel;

	// Confirmation modals
	int hideStopRecordingModal;
};

struct SettingsDialogGroupBoxes {
	void *showHideButtonsGrpPtr;
	void *showHideLabelsGrpPtr;
	void *showHideConfirmationModalsGrpPtr;
};

struct SettingsDialogCheckboxes {
	void *showHideFloatChkPtr;
	void *showHideStudioModeChkPtr;
	void *showHideVirtualCameraChkPtr;
	void *showHideReplayBufferChkPtr;
	void *showHideStartRecordingChkPtr;
	void *showHideStartStreamingChkPtr;
	void *showHideStreamingChkPtr;
	void *showHideRecordingChkPtr;
	void *showHideCurrentSceneChkPtr;
	void *showHideCPUChkPtr;
	void *showHideFPSChkPtr;
	void *showHideDroppedFramesChkPtr;
	void *showHideKbpsChkPtr;
	void *showHideStopRecordingChkPtr;
};

struct SettingsDialogLabels {
	void *creditsLblPtr;
	void *contributorsLblPtr;
	void *versionValueLblPtr;
};

struct PluginSettingsDialog {
	void *pluginSettingsDialogPtr;
	SettingsDialogGroupBoxes groupBoxes;
	SettingsDialogLabels labels;
	SettingsDialogCheckboxes checkBoxes;
};

extern "C" {
SHIM_API PluginSettingsDialog create_settings_dialog(const PluginSettings *current);
SHIM_API void destroy_settings_dialog(void *dlg);
SHIM_API int show_settings_dialog(void *dlg, PluginSettings *result);
}
