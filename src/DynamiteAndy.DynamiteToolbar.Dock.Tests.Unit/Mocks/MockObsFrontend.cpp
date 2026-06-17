#include "MockObsFrontend.h"

// Global mock instance pointer — tests set this in SetUp
IObsFrontend *g_mockObsFrontend = nullptr;

// ---------------------------------------------------------------------------
// C-linkage stubs — these replace obs-frontend-api at link time for the
// test binary. Every OBS frontend call used by the shims is forwarded to
// the active mock instance.
// ---------------------------------------------------------------------------

extern "C" {

void obs_frontend_add_dock_by_id(const char *id, const char *title, void *widget)
{
	if (g_mockObsFrontend) {
		g_mockObsFrontend->add_dock_by_id(id, title, widget);
	}
}

void obs_frontend_remove_dock(const char *id)
{
	if (g_mockObsFrontend) {
		g_mockObsFrontend->remove_dock(id);
	}
}

void *obs_frontend_add_tools_menu_qaction(const char *label)
{
	if (g_mockObsFrontend) {
		return g_mockObsFrontend->add_tools_menu_qaction(label);
	}

	return nullptr;
}

void *obs_frontend_get_main_window()
{
	if (g_mockObsFrontend) {
		return g_mockObsFrontend->get_main_window();
	}

	return nullptr;
}

} // extern "C"
