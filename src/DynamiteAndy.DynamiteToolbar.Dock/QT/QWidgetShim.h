#pragma once
#include "../ModuleShim.h"

extern "C" {
SHIM_API void qwidget_set_visible(void *ptr, int visible);
SHIM_API void qwidget_set_enabled(void *ptr, int enabled);
SHIM_API void qwidget_set_tooltip(void *ptr, const char *text);
}
