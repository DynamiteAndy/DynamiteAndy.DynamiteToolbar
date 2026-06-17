#pragma once

#include <gmock/gmock.h>

// ---------------------------------------------------------------------------
// Abstract interface covering every OBS frontend call used by the shims.
// Tests set g_mockObsFrontend to a MockObsFrontend instance before calling
// any shim that reaches OBS.
// ---------------------------------------------------------------------------
class IObsFrontend {
public:
	virtual ~IObsFrontend() = default;

	// DockShim
	virtual void add_dock_by_id(const char *id, const char *title, void *widget) = 0;
	virtual void remove_dock(const char *id) = 0;

	// ToolsMenuShim
	virtual void *add_tools_menu_qaction(const char *label) = 0;

	// QMessageBoxShim
	virtual void *get_main_window() = 0;
};

class MockObsFrontend : public IObsFrontend {
public:
	MOCK_METHOD(void, add_dock_by_id, (const char *, const char *, void *), (override));
	MOCK_METHOD(void, remove_dock, (const char *), (override));
	MOCK_METHOD(void *, add_tools_menu_qaction, (const char *), (override));
	MOCK_METHOD(void *, get_main_window, (), (override));
};

// Global pointer — set in each test's SetUp, cleared in TearDown
extern IObsFrontend *g_mockObsFrontend;
