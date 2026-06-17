#pragma once
#include "../ModuleShim.h"

extern "C" {
SHIM_API void qabstract_button_set_text(void *ptr, const char *text);
SHIM_API void qabstract_button_set_checked(void *ptr, int checked);
SHIM_API void qabstract_button_on_clicked(void *ptr, CallbackActionBool cb);
}
