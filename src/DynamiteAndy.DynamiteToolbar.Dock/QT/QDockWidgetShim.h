#pragma once
#include "../ModuleShim.h"

extern "C" {
SHIM_API void qdock_widget_set_floating(void *ptr, int floating);
SHIM_API void qdock_widget_on_toplevelchanged(void *ptr, CallbackActionBool cb);
}
