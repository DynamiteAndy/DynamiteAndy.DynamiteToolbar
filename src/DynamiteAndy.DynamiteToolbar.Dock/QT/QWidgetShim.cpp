#include "QWidgetShim.h"
#include "QObjectUtils.h"
#include <QtWidgets/QWidget>

extern "C" {
SHIM_API void qwidget_set_visible(void *ptr, int visible)
{
	update_bool<QWidget>(ptr, visible, [](QWidget *w, bool b) { w->setVisible(b); });
}

SHIM_API void qwidget_set_enabled(void *ptr, int enabled)
{
	update_bool<QWidget>(ptr, enabled, [](QWidget *w, bool b) { w->setEnabled(b); });
}

SHIM_API void qwidget_set_tooltip(void *ptr, const char *text)
{
	update_string<QWidget>(ptr, text, [](QWidget *w, QString s) { w->setToolTip(s); });
}
}
