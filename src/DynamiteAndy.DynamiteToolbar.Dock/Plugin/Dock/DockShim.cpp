#include "DockShim.h"
#include "DockWrapperWidget.h"
#include <obs-frontend-api.h>
#include <mutex>
#include <memory>

#include <QtCore/QPointer>

struct ToolbarState {
	QPointer<DockWrapperWidget> dock;
	PluginDock toolbarData = {};
};

static std::unique_ptr<ToolbarState> g_state;
static std::mutex g_state_mutex;

extern "C" {

SHIM_API void create_dock(const char *title, PluginDock *out)
{
	std::lock_guard<std::mutex> lock(g_state_mutex);

	if (g_state && g_state->dock) {
		if (out)
			*out = g_state->toolbarData;
		return;
	}

	if (!g_state)
		g_state = std::make_unique<ToolbarState>();

	DockWrapperWidget *dock = new DockWrapperWidget();
	dock->setWindowTitle(QString::fromUtf8(title));
	dock->populate(g_state->toolbarData);

	g_state->dock = dock;

	QByteArray idBytes = dock->objectName().toUtf8();
	QByteArray titleBytes = dock->windowTitle().toUtf8();
	obs_frontend_add_dock_by_id(idBytes.constData(), titleBytes.constData(), dock);

	if (out)
		*out = g_state->toolbarData;
}

SHIM_API void destroy_dock()
{
	std::lock_guard<std::mutex> lock(g_state_mutex);
	if (!g_state)
		return;

	if (g_state->dock)
		obs_frontend_remove_dock(g_state->dock->objectName().toUtf8().constData());

	g_state.reset();
}
}
