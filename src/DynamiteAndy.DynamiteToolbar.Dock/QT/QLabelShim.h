#pragma once
#include "../ModuleShim.h"

extern "C" {
SHIM_API void qlabel_set_text(void *ptr, const char *text);
}
