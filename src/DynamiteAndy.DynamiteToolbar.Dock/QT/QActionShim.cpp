#include "QActionShim.h"
#include "QObjectUtils.h"
#include <QtGui/QAction>

extern "C" {
SHIM_API void qaction_set_text(void *ptr, const char *text)
{
	update_string<QAction>(ptr, text, [](QAction *a, QString s) { a->setText(s); });
}

SHIM_API void qaction_set_enabled(void *ptr, int enabled)
{
	update_bool<QAction>(ptr, enabled, [](QAction *a, bool b) { a->setEnabled(b); });
}

SHIM_API void qaction_set_visible(void *ptr, int visible)
{
	update_bool<QAction>(ptr, visible, [](QAction *a, bool b) { a->setVisible(b); });
}

SHIM_API void qaction_on_triggered(void *ptr, CallbackActionBool cb)
{
	if (!ptr || !cb)
		return;

	auto *action = static_cast<QAction *>(ptr);

	update_callback<QAction>(action, &QAction::triggered, [cb](bool isChecked) { cb(isChecked ? 1 : 0); });
}
}
