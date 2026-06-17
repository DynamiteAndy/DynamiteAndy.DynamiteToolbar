#include "QLabelShim.h"
#include "QObjectUtils.h"
#include <QtWidgets/QLabel>

extern "C" {
SHIM_API void qlabel_set_text(void *ptr, const char *text)
{
	update_string<QLabel>(ptr, text, [](QLabel *l, QString s) { l->setText(s); });
}
}
