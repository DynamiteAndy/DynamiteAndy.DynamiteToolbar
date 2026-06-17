#pragma once

#include "ui_plugin-dock-wrapper.h"
#include "../Types.h"

#include <QDockWidget>
#include <QPointer>

class DockHeaderWidget;

class DockWrapperWidget : public QDockWidget, public Ui::DockWrapper {
	Q_OBJECT

public:
	explicit DockWrapperWidget();

	void populate(PluginDock &t);

protected:
	bool event(QEvent *ev) override;

private:
	void configureWrapper(QDockWidget *wrapper);

	DockHeaderWidget *m_header = nullptr;
};
