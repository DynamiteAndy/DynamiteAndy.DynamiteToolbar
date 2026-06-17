#pragma once

#include "ui_plugin-toolbar.h"
#include "FrameMonitorDockWidget.h"
#include "../Types.h"

#include <QWidget>
#include <QResizeEvent>

class ToolbarWidget : public QWidget, public Ui::Toolbar {
	Q_OBJECT

public:
	explicit ToolbarWidget(QWidget *parent = nullptr);

	void refreshIcons();
	void populate(PluginDock &t);

	bool hasHeightForWidth() const override;
	int heightForWidth(int width) const override;
	QSize sizeHint() const override;

protected:
	void resizeEvent(QResizeEvent *event) override;
	void changeEvent(QEvent *event) override;
	bool event(QEvent *event) override;

private:
	void applyHeightForWidth(int width);
	mutable int m_lastNeededHeight = -1;
};
