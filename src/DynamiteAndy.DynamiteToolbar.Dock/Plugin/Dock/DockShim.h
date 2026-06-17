#pragma once

#include "../../ModuleShim.h"
#include "../Types.h"

extern "C" {
SHIM_API void create_dock(const char *title, PluginDock *out);
SHIM_API void destroy_dock();
}
