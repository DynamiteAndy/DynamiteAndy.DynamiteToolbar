#include "DockWrapperWidget.h"
#include "DockHeaderWidget.h"
#include "FrameMonitorDockWidget.h"

#include <QPointer>
#include <QMetaObject>
#include <QEvent>
#include <QDockWidget>

DockWrapperWidget::DockWrapperWidget() : QDockWidget()
{
	setupUi(this);

	setTitleBarWidget(new QWidget());

	m_header = new DockHeaderWidget(this);
}

void DockWrapperWidget::populate(PluginDock &t)
{
	t.pluginDockPtr = static_cast<void *>(this);
	toolbar->populate(t);
}

bool DockWrapperWidget::event(QEvent *ev)
{
	if (ev->type() == QEvent::ParentChange) {
		if (auto *wrapper = qobject_cast<QDockWidget *>(parentWidget())) {
			QMetaObject::invokeMethod(
				this,
				[this, guard = QPointer(wrapper)]() {
					if (guard)
						configureWrapper(guard);
				},
				Qt::QueuedConnection);
		}
	}

	return QDockWidget::event(ev);
}

void DockWrapperWidget::configureWrapper(QDockWidget *wrapper)
{
	QMetaObject::invokeMethod(
		this,
		[this, guard = QPointer(wrapper)]() {
			if (guard) {
				m_header->installOn(guard, this);
			}
		},
		Qt::QueuedConnection);
}
