#include "ModuleShim.h"
#include <obs.h>

#if defined(_WIN32)

#include <windows.h>
#include <string>

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	if (fdwReason == DLL_PROCESS_ATTACH) {
		SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);

		wchar_t path[MAX_PATH];

		if (GetModuleFileNameW(NULL, path, MAX_PATH)) {
			std::wstring exePath(path);
			auto slash = exePath.rfind(L'\\');

			if (slash != std::wstring::npos) {
				AddDllDirectory(exePath.substr(0, slash).c_str());
			}
		}
	}

	return TRUE;
}

#endif
