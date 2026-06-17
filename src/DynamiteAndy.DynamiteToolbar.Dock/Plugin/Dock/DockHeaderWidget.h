#pragma once

#include "ui_plugin-dock-header.h"
#include <QtWidgets/QDockWidget>
#include <QtWidgets/QWidget>

class DockHeaderWidget : public QWidget, public Ui::DockHeader {
	Q_OBJECT

public:
	explicit DockHeaderWidget(QWidget *parent = nullptr) : QWidget(parent) { setupUi(this); }

	void installOn(QDockWidget *wrapper, QDockWidget *inner = nullptr)
	{
		if (inner)
			wrapper->setAllowedAreas(inner->allowedAreas());
		wrapper->setTitleBarWidget(this);
		syncFrom(wrapper);

		disconnect(wrapper, &QDockWidget::topLevelChanged, this, nullptr);
		connect(wrapper, &QDockWidget::topLevelChanged, this, [this, wrapper](bool) { syncFrom(wrapper); });

		disconnect(btnClose, &QToolButton::clicked, nullptr, nullptr);
		connect(btnClose, &QToolButton::clicked, wrapper, &QDockWidget::close);
	}

	void setTitle(const QString &title) { lblTitle->setText(title); }

	void setFloating(bool floating)
	{
		if (floating) {
			lblTitle->show();
			btnClose->show();
			setFixedHeight(sizeHint().height());
		} else {
			lblTitle->hide();
			btnClose->hide();
			setFixedHeight(0);
		}
	}

private:
	void syncFrom(QDockWidget *w)
	{
		setTitle(w->windowTitle());
		setFloating(w->isFloating());
	}
};
