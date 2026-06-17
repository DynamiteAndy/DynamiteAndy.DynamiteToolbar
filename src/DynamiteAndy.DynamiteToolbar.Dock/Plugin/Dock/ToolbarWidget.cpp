#include "ToolbarWidget.h"

#include "../../QT/QIconUtils.h"

#include <QToolButton>
#include <QPalette>
#include <QPointer>

ToolbarWidget::ToolbarWidget(QWidget *parent) : QWidget(parent)
{
	setupUi(this);
	refreshIcons();
}

void ToolbarWidget::refreshIcons()
{
	const QPalette pal = palette();

	for (QToolButton *btn : findChildren<QToolButton *>()) {
		const QVariant pathVar = btn->property("svgPath");
		if (pathVar.isValid()) {
			btn->setIcon(QIconUtils::createIcon(pathVar.toString(), pal));
			btn->setIconSize(QSize(16, 16));
		}
	}
}

void ToolbarWidget::populate(PluginDock &t)
{
	t.monitor = frameMonitor->populate();

	t.buttons = {btnFloat, btnStudioMode, btnVirtualCamera, btnReplayBuffer, btnRecord, btnRecordPaused, btnStream};

	t.spacers = {spacerFloat,         spacerStudioMode,   spacerMonitor,
		     spacerVirtualCamera, spacerReplayBuffer, spacerRecord};
}

void ToolbarWidget::changeEvent(QEvent *event)
{
	if (event->type() == QEvent::PaletteChange)
		refreshIcons();

	QWidget::changeEvent(event);
}

bool ToolbarWidget::hasHeightForWidth() const
{
	const bool v = frameMonitor && frameMonitor->hasHeightForWidth();
	return v;
}

int ToolbarWidget::heightForWidth(int width) const
{
	if (!frameMonitor || !frameMonitor->hasHeightForWidth())
		return QWidget::heightForWidth(width);

	int fixedColumnsWidth = width - frameMonitor->width();
	int monitorWidth = qMax(0, width - fixedColumnsWidth);

	if (this->width() > 0 && frameMonitor->width() > 0) {
		int otherWidth = this->width() - frameMonitor->width();
		monitorWidth = qMax(0, width - otherWidth);
	} else {
		monitorWidth = width;
	}

	const int monitorHeight = frameMonitor->heightForWidth(monitorWidth);
	int total = monitorHeight;

	if (layout()) {
		const QMargins lm = layout()->contentsMargins();
		total += lm.top() + lm.bottom();
	}

	const QMargins wm = contentsMargins();
	total += wm.top() + wm.bottom();

	return total;
}

QSize ToolbarWidget::sizeHint() const
{
	QSize s = QWidget::sizeHint();
	if (hasHeightForWidth())
		s.setHeight(heightForWidth(s.width()));

	return s;
}

void ToolbarWidget::applyHeightForWidth(int width)
{
	if (!frameMonitor || !frameMonitor->hasHeightForWidth())
		return;

	const int needed = heightForWidth(width);

	if (needed == m_lastNeededHeight)
		return;

	m_lastNeededHeight = needed;

	setFixedHeight(needed);
	updateGeometry();

	QPointer<QWidget> guard(this);
	QMetaObject::invokeMethod(
		this,
		[guard]() {
			if (!guard)
				return;
			QWidget *self = guard;
			if (self->parentWidget()) {
				self->parentWidget()->updateGeometry();
			}
		},
		Qt::QueuedConnection);
}

void ToolbarWidget::resizeEvent(QResizeEvent *event)
{
	QWidget::resizeEvent(event);
	applyHeightForWidth(event->size().width());
}

bool ToolbarWidget::event(QEvent *event)
{
	if (event->type() == QEvent::LayoutRequest)
		applyHeightForWidth(width());

	return QWidget::event(event);
}
