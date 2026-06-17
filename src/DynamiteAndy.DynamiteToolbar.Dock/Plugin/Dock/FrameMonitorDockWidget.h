#pragma once

#include "ui_plugin-monitor-panel.h"
#include "../Types.h"

#include <QtWidgets/QFrame>
#include <QtWidgets/QLabel>
#include <QResizeEvent>

class FrameMonitorDockWidget : public QFrame, public Ui::QFrameMonitor {
	Q_OBJECT

public:
	explicit FrameMonitorDockWidget(QWidget *parent = nullptr);

	DockFrameMonitor populate();

	bool hasHeightForWidth() const override;
	int heightForWidth(int width) const override;
	QSize sizeHint() const override;

protected:
	void resizeEvent(QResizeEvent *event) override;
};
