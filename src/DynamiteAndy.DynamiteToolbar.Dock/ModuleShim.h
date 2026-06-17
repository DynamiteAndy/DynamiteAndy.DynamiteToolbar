#pragma once

#if defined(_WIN32)
#ifdef DYNAMITEANDYDYNAMITETOOLBARDOCK_EXPORTS
#define SHIM_API __declspec(dllexport)
#else
#define SHIM_API __declspec(dllimport)
#endif
#else
#define SHIM_API __attribute__((visibility("default")))
#endif

typedef void (*CallbackActionVoid)(void *ctx);
typedef void (*CallbackActionBool)(int);
