#include "FlowLayout.h"
#include <QtWidgets>

// QWidgetItem returns (0,0) for unshown/unparented widgets, so query
// the widget directly when available.
static QSize itemSizeHint(QLayoutItem *item)
{
	if (QWidget *w = item->widget()) {
		if (!w->isVisible())
			return QSize(0, 0);
		return w->sizeHint();
	}
	return item->sizeHint();
}

static QSize itemMinimumSize(QLayoutItem *item)
{
	if (QWidget *w = item->widget()) {
		if (!w->isVisible())
			return QSize(0, 0);
		return w->minimumSize();
	}
	return item->minimumSize();
}

FlowLayout::FlowLayout(QWidget *parent, int margin, int hSpacing, int vSpacing)
	: QLayout(parent),
	  m_hSpace(hSpacing),
	  m_vSpace(vSpacing)
{
	setContentsMargins(margin, margin, margin, margin);
}

FlowLayout::FlowLayout(int margin, int hSpacing, int vSpacing) : m_hSpace(hSpacing), m_vSpace(vSpacing)
{
	setContentsMargins(margin, margin, margin, margin);
}

FlowLayout::~FlowLayout()
{
	QLayoutItem *item;
	while ((item = takeAt(0)))
		delete item;
}

void FlowLayout::addItem(QLayoutItem *item)
{
	itemList.append(item);
}

int FlowLayout::horizontalSpacing() const
{
	if (m_hSpace >= 0)
		return m_hSpace;
	return smartSpacing(QStyle::PM_LayoutHorizontalSpacing);
}

int FlowLayout::verticalSpacing() const
{
	if (m_vSpace >= 0)
		return m_vSpace;
	return smartSpacing(QStyle::PM_LayoutVerticalSpacing);
}

int FlowLayout::count() const
{
	return itemList.size();
}

QLayoutItem *FlowLayout::itemAt(int index) const
{
	return itemList.value(index);
}

QLayoutItem *FlowLayout::takeAt(int index)
{
	if (index >= 0 && index < itemList.size())
		return itemList.takeAt(index);
	return nullptr;
}

Qt::Orientations FlowLayout::expandingDirections() const
{
	return Qt::Horizontal;
}

bool FlowLayout::hasHeightForWidth() const
{
	return true;
}

int FlowLayout::heightForWidth(int width) const
{
	return doLayout(QRect(0, 0, width, 0), true);
}

QSize FlowLayout::sizeHint() const
{
	// Compute the total width needed to lay all visible items out on a single row,
	// so the parent container requests enough space before wrapping begins.
	const QMargins m = contentsMargins();
	int totalWidth = m.left() + m.right();
	int maxHeight = 0;
	const int hSpace = horizontalSpacing();
	bool first = true;
	for (QLayoutItem *item : itemList) {
		QSize s = itemSizeHint(item);
		if (s.isEmpty())
			continue;
		if (!first)
			totalWidth += (hSpace >= 0 ? hSpace : 0);
		totalWidth += s.width();
		maxHeight = qMax(maxHeight, s.height());
		first = false;
	}
	return QSize(totalWidth, maxHeight + m.top() + m.bottom());
}

QSize FlowLayout::minimumSize() const
{
	QSize size;
	for (QLayoutItem *item : itemList)
		size = size.expandedTo(itemMinimumSize(item));

	const QMargins m = contentsMargins();
	size += QSize(m.left() + m.right(), m.top() + m.bottom());
	return size;
}

void FlowLayout::setGeometry(const QRect &rect)
{
	QLayout::setGeometry(rect);
	doLayout(rect, false);
}

int FlowLayout::doLayout(const QRect &rect, bool testOnly) const
{
	int left, top, right, bottom;
	getContentsMargins(&left, &top, &right, &bottom);
	QRect effectiveRect = rect.adjusted(+left, +top, -right, -bottom);
	int y = effectiveRect.y();
	const int hSpace = horizontalSpacing();
	const int vSpace = verticalSpacing();
	const int availableWidth = effectiveRect.width();

	// Collect visible items into rows first, then center each row
	struct Row {
		QList<QLayoutItem *> items;
		int width = 0;
	};

	QList<Row> rows;
	Row currentRow;

	for (QLayoutItem *item : itemList) {
		QSize itemSize = itemSizeHint(item);
		if (itemSize.isEmpty())
			continue;

		int itemWidth = itemSize.width() + (hSpace >= 0 ? hSpace : 0);

		bool wouldOverflow = currentRow.width + itemSize.width() > availableWidth &&
				     !currentRow.items.isEmpty();
		if (wouldOverflow) {
			rows.append(currentRow);
			currentRow = Row{};
		}

		currentRow.items.append(item);
		currentRow.width += itemWidth;
	}
	if (!currentRow.items.isEmpty())
		rows.append(currentRow);

	for (const Row &row : rows) {
		// Compute this row height
		int rowHeight = 0;
		for (QLayoutItem *item : row.items)
			rowHeight = qMax(rowHeight, itemSizeHint(item).height());

		// Center the row horizontally: subtract trailing hSpace added after last item
		int rowWidth = row.width - (hSpace >= 0 ? hSpace : 0);
		int x = effectiveRect.x() + (availableWidth - rowWidth) / 2;

		if (!testOnly) {
			int px = x;
			for (QLayoutItem *item : row.items) {
				QSize itemSize = itemSizeHint(item);
				item->setGeometry(QRect(QPoint(px, y), itemSize));
				px += itemSize.width() + (hSpace >= 0 ? hSpace : 0);
			}
		}

		y += rowHeight + (vSpace >= 0 ? vSpace : 0);
	}

	// Remove the trailing vSpace added after the last row
	if (!rows.isEmpty() && vSpace >= 0)
		y -= vSpace;

	return y - rect.y() + bottom;
}

int FlowLayout::smartSpacing(QStyle::PixelMetric pm) const
{
	QObject *parent = this->parent();
	if (!parent)
		return -1;
	if (parent->isWidgetType()) {
		QWidget *pw = static_cast<QWidget *>(parent);
		return pw->style()->pixelMetric(pm, nullptr, pw);
	} else {
		return static_cast<QLayout *>(parent)->spacing();
	}
}
