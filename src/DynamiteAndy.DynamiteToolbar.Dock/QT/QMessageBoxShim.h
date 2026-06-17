#pragma once
#include "../ModuleShim.h"

extern "C" {
SHIM_API bool show_confirmation(const char *title, const char *text);
}
