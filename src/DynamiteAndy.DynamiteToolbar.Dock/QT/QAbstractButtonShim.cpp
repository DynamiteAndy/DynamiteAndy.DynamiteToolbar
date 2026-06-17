#include "QAbstractButtonShim.h"
#include "QObjectUtils.h"
#include <QtWidgets/QAbstractButton>

extern "C" {
SHIM_API void qabstract_button_set_text(void *ptr, const char *text)
{
	update_string<QAbstractButton>(ptr, text, [](QAbstractButton *b, QString s) { b->setText(s); });
}

SHIM_API void qabstract_button_set_checked(void *ptr, int checked)
{
	update_bool<QAbstractButton>(ptr, checked, [](QAbstractButton *b, bool val) { b->setChecked(val); });
}

SHIM_API void qabstract_button_on_clicked(void *ptr, CallbackActionBool cb)
{
	if (!ptr || !cb)
		return;

	auto *button = static_cast<QAbstractButton *>(ptr);

	update_callback<QAbstractButton>(button, &QAbstractButton::clicked,
					 [cb](bool isChecked) { cb(isChecked ? 1 : 0); });
}
}
