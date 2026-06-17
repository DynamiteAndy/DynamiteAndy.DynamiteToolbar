#include "ToolsMenuShim.h"
#include <obs-frontend-api.h>
#include <QtGui/QAction>

extern "C" {

SHIM_API void *add_tools_menu_qaction(const char *actionLabel)
{
	return obs_frontend_add_tools_menu_qaction(actionLabel);
}
}
