#pragma once
#include "../ModuleShim.h"

extern "C" {
SHIM_API void qaction_set_text(void *ptr, const char *text);
SHIM_API void qaction_set_enabled(void *ptr, int enabled);
SHIM_API void qaction_set_visible(void *ptr, int visible);
SHIM_API void qaction_on_triggered(void *ptr, CallbackActionBool cb);
}
