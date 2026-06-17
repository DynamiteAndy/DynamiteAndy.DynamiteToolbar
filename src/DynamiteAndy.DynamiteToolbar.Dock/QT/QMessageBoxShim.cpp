#include "QMessageBoxShim.h"
#include "QThreadUtils.h"
#include <QMessageBox>
#include <QMainWindow>
#include <obs-frontend-api.h>

static bool dispatch_message_box(const char *title, const char *text, QMessageBox::Icon icon,
				 QMessageBox::StandardButtons buttons)
{
	return runOnMainThread([&]() {
		QMainWindow *mainWin = static_cast<QMainWindow *>(obs_frontend_get_main_window());

		QMessageBox msg(mainWin);
		msg.setWindowTitle(QString::fromUtf8(title));
		msg.setText(QString::fromUtf8(text));
		msg.setIcon(icon);
		msg.setStandardButtons(buttons);

		int result = msg.exec();
		return (result == QMessageBox::Yes || result == QMessageBox::Ok);
	});
}

extern "C" {
SHIM_API bool show_confirmation(const char *title, const char *text)
{
	return dispatch_message_box(title, text, QMessageBox::Question, QMessageBox::Yes | QMessageBox::No);
}
}
