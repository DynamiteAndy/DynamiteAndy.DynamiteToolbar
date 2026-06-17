#include "QDockWidgetShim.h"
#include "QObjectUtils.h"
#include <QtWidgets/QDockWidget>

extern "C" {
SHIM_API void qdock_widget_set_floating(void *ptr, int floating)
{
	update_bool<QDockWidget>(ptr, floating, [](QDockWidget *d, bool f) {
		auto *wrapper = qobject_cast<QDockWidget *>(d->parentWidget());
		QDockWidget *target = wrapper ? wrapper : d;

		if (target->isFloating() == f)
			return;

		target->setFloating(f);
	});
}

SHIM_API void qdock_widget_on_toplevelchanged(void *ptr, CallbackActionBool cb)
{
	if (!ptr || !cb)
		return;

	auto *dock = static_cast<QDockWidget *>(ptr);

	QMetaObject::invokeMethod(
		dock,
		[dock, cb]() {
			auto *wrapper = qobject_cast<QDockWidget *>(dock->parentWidget());
			if (!wrapper) {
				return;
			}

			cb(wrapper->isFloating() ? 1 : 0);

			update_callback<QDockWidget>(wrapper, &QDockWidget::topLevelChanged,
						     [cb](bool topLevel) { cb(topLevel ? 1 : 0); });
		},
		Qt::QueuedConnection);
}
}
