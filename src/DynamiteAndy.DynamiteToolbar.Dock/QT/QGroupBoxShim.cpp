#include "QGroupBoxShim.h"
#include "QObjectUtils.h"
#include <QtWidgets/QGroupBox>

extern "C" {

SHIM_API void qgroup_box_set_title(void *ptr, const char *title)
{
	update_string<QGroupBox>(ptr, title, [](QGroupBox *g, QString s) { g->setTitle(s); });
}
}
