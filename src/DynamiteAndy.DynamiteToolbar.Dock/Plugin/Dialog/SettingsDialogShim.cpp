#include "../../ModuleShim.h"
#include "../../QT/QThreadUtils.h"
#include "SettingsDialogShim.h"
#include "ui_plugin-settings-dialog.h"
#include <array>

class SettingsDialog : public QDialog, public Ui::DialogPluginSettings {
public:
	explicit SettingsDialog(const PluginSettings &current, QWidget *parent = nullptr) : QDialog(parent)
	{
		setupUi(this);
		writeToUi(current);

		connect(dialogButtonBox, &QDialogButtonBox::accepted, this, &QDialog::accept);
		connect(dialogButtonBox, &QDialogButtonBox::rejected, this, &QDialog::reject);
	}

	void writeToUi(const PluginSettings &s)
	{
		PluginSettings temp = s;
		sync(temp, false);
	}

	void readFromUi(PluginSettings &s) { sync(s, true); }

private:
	void sync(PluginSettings &s, bool saveFromUi)
	{
		struct Map {
			QCheckBox *cb;
			int &val;
		};

		const std::array<Map, 14> maps = {{{cbxShowHideFloat, s.hideFloat},
						   {cbxShowHideStudioMode, s.hideStudioMode},
						   {cbxShowHideVirtualCamera, s.hideVirtualCamera},
						   {cbxShowHideReplayBuffer, s.hideReplayBuffer},
						   {cbxShowHideStartRecording, s.hideStartRecording},
						   {cbxShowHideStartStreaming, s.hideStartStreaming},
						   {cbxShowHideStreaming, s.hideStreamingLabel},
						   {cbxShowHideRecording, s.hideRecordingLabel},
						   {cbxShowHideCurrentScene, s.hideCurrentSceneLabel},
						   {cbxShowHideCPU, s.hideCpuLabel},
						   {cbxShowHideFPS, s.hideFpsLabel},
						   {cbxShowHideDroppedFrames, s.hideDroppedFramesLabel},
						   {cbxShowHideKbps, s.hideKbpsLabel},
						   {cbxShowHideStopRecording, s.hideStopRecordingModal}}};

		for (const auto &m : maps) {
			if (saveFromUi) {
				m.val = m.cb->isChecked() ? 1 : 0;
			} else {
				m.cb->setChecked(m.val != 0);
			}
		}
	}
};

extern "C" {

SHIM_API PluginSettingsDialog create_settings_dialog(const PluginSettings *current)
{
	return runOnMainThread([&]() {
		PluginSettingsDialog res = {};
		if (!current)
			return res;

		auto *dlg = new SettingsDialog(*current);
		res.pluginSettingsDialogPtr = dlg;

		res.groupBoxes = {dlg->grpShowHideButtons, dlg->grpShowHideLabels, dlg->grpShowHideConfirmationModals};

		res.checkBoxes = {dlg->cbxShowHideFloat,
				  dlg->cbxShowHideStudioMode,
				  dlg->cbxShowHideVirtualCamera,
				  dlg->cbxShowHideReplayBuffer,
				  dlg->cbxShowHideStartRecording,
				  dlg->cbxShowHideStartStreaming,
				  dlg->cbxShowHideStreaming,
				  dlg->cbxShowHideRecording,
				  dlg->cbxShowHideCurrentScene,
				  dlg->cbxShowHideCPU,
				  dlg->cbxShowHideFPS,
				  dlg->cbxShowHideDroppedFrames,
				  dlg->cbxShowHideKbps,
				  dlg->cbxShowHideStopRecording};

		dlg->lblVersionValue->setText(QString::fromUtf8("v" PROJECT_VERSION));
		res.labels = {dlg->lblCreditsName, dlg->lblContributorsName, dlg->lblVersionValue};

		return res;
	});
}

SHIM_API void destroy_settings_dialog(void *dlg)
{
	if (!dlg) {
		return;
	}

	runOnMainThread([&]() { delete static_cast<SettingsDialog *>(dlg); });
}

SHIM_API int show_settings_dialog(void *dlg, PluginSettings *result)
{
	if (!dlg || !result) {
		return 0;
	}

	return runOnMainThread([&]() {
		auto *dialog = static_cast<SettingsDialog *>(dlg);

		if (dialog->exec() == QDialog::Accepted) {
			dialog->readFromUi(*result);
			return 1;
		}

		return 0;
	});
}
}
