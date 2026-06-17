#include <gtest/gtest.h>
#include <QApplication>
#include <QLabel>

#include "Plugin/FlowLayout.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class FlowLayoutTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}

	void SetUp() override
	{
		_container = new QWidget();
		_container->show();
	}

	void TearDown() override { delete _container; }

	QLabel *makeItem(int w, int h)
	{
		auto *lbl = new QLabel(_container);
		lbl->setFixedSize(w, h);
		lbl->show();
		lbl->ensurePolished();
		return lbl;
	}

	QWidget *_container = nullptr;
};

TEST_F(FlowLayoutTest, EmptyLayoutHasCountZero)
{
	// Arrange + Act
	FlowLayout layout;

	// Assert
	EXPECT_EQ(layout.count(), 0);
}

TEST_F(FlowLayoutTest, AddItemIncrementsCount)
{
	// Arrange
	FlowLayout layout;

	// Act + Assert
	layout.addWidget(makeItem(50, 20));
	EXPECT_EQ(layout.count(), 1);
	layout.addWidget(makeItem(50, 20));
	EXPECT_EQ(layout.count(), 2);
}

TEST_F(FlowLayoutTest, ItemAtReturnsCorrectWidget)
{
	// Arrange
	FlowLayout layout;
	auto *lbl = makeItem(40, 20);

	// Act
	layout.addWidget(lbl);

	// Assert
	ASSERT_NE(layout.itemAt(0), nullptr);
	EXPECT_EQ(layout.itemAt(0)->widget(), lbl);
}

TEST_F(FlowLayoutTest, ItemAtOutOfRangeReturnsNull)
{
	// Arrange + Act
	FlowLayout layout;

	// Assert
	EXPECT_EQ(layout.itemAt(99), nullptr);
}

TEST_F(FlowLayoutTest, TakeAtRemovesItem)
{
	// Arrange
	FlowLayout layout;
	layout.addWidget(makeItem(40, 20));

	// Act
	auto *item = layout.takeAt(0);

	// Assert
	EXPECT_NE(item, nullptr);
	EXPECT_EQ(layout.count(), 0);
	delete item;
}

TEST_F(FlowLayoutTest, TakeAtOutOfRangeReturnsNull)
{
	// Arrange + Act
	FlowLayout layout;

	// Assert
	EXPECT_EQ(layout.takeAt(5), nullptr);
}

TEST_F(FlowLayoutTest, TakeAtNegativeIndexReturnsNull)
{
	// Arrange
	FlowLayout layout;
	layout.addWidget(makeItem(40, 20));

	// Act + Assert
	EXPECT_EQ(layout.takeAt(-1), nullptr);
	EXPECT_EQ(layout.count(), 1); // item not removed
}

TEST_F(FlowLayoutTest, HorizontalSpacingReturnsConstructorValue)
{
	// Arrange + Act
	FlowLayout layout(nullptr, 0, 8, 4);

	// Assert
	EXPECT_EQ(layout.horizontalSpacing(), 8);
}

TEST_F(FlowLayoutTest, VerticalSpacingReturnsConstructorValue)
{
	// Arrange + Act
	FlowLayout layout(nullptr, 0, 4, 6);

	// Assert
	EXPECT_EQ(layout.verticalSpacing(), 6);
}

TEST_F(FlowLayoutTest, HorizontalSpacingDefaultNoParentReturnsNegOne)
{
	// Arrange + Act
	FlowLayout layout; // m_hSpace = -1

	// Assert
	EXPECT_EQ(layout.horizontalSpacing(), -1);
}

TEST_F(FlowLayoutTest, VerticalSpacingDefaultNoParentReturnsNegOne)
{
	// Arrange + Act
	FlowLayout layout; // m_vSpace = -1

	// Assert
	EXPECT_EQ(layout.verticalSpacing(), -1);
}

TEST_F(FlowLayoutTest, HasHeightForWidth)
{
	// Arrange + Act
	FlowLayout layout;

	// Assert
	EXPECT_TRUE(layout.hasHeightForWidth());
}

TEST_F(FlowLayoutTest, ExpandsHorizontally)
{
	// Arrange + Act
	FlowLayout layout;

	// Assert
	EXPECT_TRUE(layout.expandingDirections() & Qt::Horizontal);
}

TEST_F(FlowLayoutTest, HeightForWidthEmptyLayout)
{
	// Arrange + Act
	FlowLayout layout(nullptr, 0, 0, 0);

	// Assert
	EXPECT_EQ(layout.heightForWidth(200), 0);
}

TEST_F(FlowLayoutTest, AllItemsFitOneRow)
{
	// Arrange — three 40px items fit comfortably in 200px on one row
	FlowLayout layout(nullptr, 0, 0, 0);
	layout.addWidget(makeItem(40, 20));
	layout.addWidget(makeItem(40, 20));
	layout.addWidget(makeItem(40, 20));

	// Act + Assert
	EXPECT_EQ(layout.heightForWidth(200), 20);
}

TEST_F(FlowLayoutTest, SingleItemFitsOneRow)
{
	// Arrange
	FlowLayout layout(nullptr, 0, 0, 0);
	layout.addWidget(makeItem(50, 30));

	// Act + Assert
	EXPECT_EQ(layout.heightForWidth(200), 30);
}

TEST_F(FlowLayoutTest, ItemsWrapOntoSecondRow)
{
	// Arrange — two 60px items in 70px must wrap onto two rows
	FlowLayout layout(nullptr, 0, 0, 0);
	layout.addWidget(makeItem(60, 20));
	layout.addWidget(makeItem(60, 20));

	// Act + Assert
	EXPECT_EQ(layout.heightForWidth(70), 40);
}

TEST_F(FlowLayoutTest, HeightForWidthRespectsVerticalSpacing)
{
	// Arrange — 5px vertical spacing between rows
	FlowLayout layout(nullptr, 0, 0, 5);
	layout.addWidget(makeItem(60, 20));
	layout.addWidget(makeItem(60, 20));

	// Act + Assert — 20 + 5 + 20 = 45
	EXPECT_EQ(layout.heightForWidth(70), 45);
}

TEST_F(FlowLayoutTest, HorizontalSpacingCausesEarlierWrap)
{
	// Arrange — three 30px items with 5px spacing; each occupies 35px,
	// so in 80px they cannot all fit on one row
	FlowLayout withSpacing(nullptr, 0, 5, 0);
	withSpacing.addWidget(makeItem(30, 20));
	withSpacing.addWidget(makeItem(30, 20));
	withSpacing.addWidget(makeItem(30, 20));

	// Act
	int h = withSpacing.heightForWidth(80);

	// Assert — wrapping must have occurred
	EXPECT_GT(h, 20);
}

TEST_F(FlowLayoutTest, MarginsAreIncludedInHeight)
{
	// Arrange — 10px margin on each side
	FlowLayout withMargin(nullptr, 10, 0, 0);
	withMargin.addWidget(makeItem(40, 20));

	// Act + Assert — content(20) + top(10) + bottom(10) = 40
	EXPECT_EQ(withMargin.heightForWidth(200), 40);
}

TEST_F(FlowLayoutTest, InvisibleItemsExcludedFromLayout)
{
	// Arrange — visible item is shown via _container; invisible is explicitly hidden
	FlowLayout layout(nullptr, 0, 0, 0);
	auto *visible = makeItem(40, 20);
	auto *invisible = makeItem(40, 20);
	invisible->hide();

	layout.addWidget(visible);
	layout.addWidget(invisible);

	// Act + Assert — only the visible item contributes
	EXPECT_EQ(layout.heightForWidth(200), 20);
}

TEST_F(FlowLayoutTest, MinimumSizeCoversLargestItem)
{
	// Arrange
	FlowLayout layout(nullptr, 0, 0, 0);
	layout.addWidget(makeItem(30, 10));
	layout.addWidget(makeItem(80, 25));

	// Act
	QSize minSz = layout.minimumSize();

	// Assert
	EXPECT_GE(minSz.width(), 80);
	EXPECT_GE(minSz.height(), 25);
}

TEST_F(FlowLayoutTest, MarginsIncludedInMinimumSize)
{
	// Arrange — 5px margin all sides
	FlowLayout layout(nullptr, 5, 0, 0);
	layout.addWidget(makeItem(40, 20));

	// Act
	QSize minSz = layout.minimumSize();

	// Assert — item + left + right margins, item + top + bottom margins
	EXPECT_GE(minSz.width(), 40 + 10);
	EXPECT_GE(minSz.height(), 20 + 10);
}

TEST_F(FlowLayoutTest, EmptyLayoutMinimumSizeEqualsMargins)
{
	// Arrange — 3px margin all sides, no items
	FlowLayout layout(nullptr, 3, 0, 0);

	// Act
	QSize minSz = layout.minimumSize();

	// Assert — QSize default-constructs to (-1,-1), so (-1+3)+(-1+3) = (5,5)
	EXPECT_EQ(minSz.width(), 5);
	EXPECT_EQ(minSz.height(), 5);
}

TEST_F(FlowLayoutTest, SizeHintWidthIsSumOfItemWidths)
{
	// Arrange
	FlowLayout layout(nullptr, 0, 0, 0);
	layout.addWidget(makeItem(30, 20));
	layout.addWidget(makeItem(50, 20));

	// Act
	QSize hint = layout.sizeHint();

	// Assert
	EXPECT_EQ(hint.width(), 80);
	EXPECT_EQ(hint.height(), 20);
}

TEST_F(FlowLayoutTest, SizeHintEmptyLayoutIsEmpty)
{
	// Arrange + Act
	FlowLayout layout(nullptr, 0, 0, 0);
	QSize hint = layout.sizeHint();

	// Assert
	EXPECT_EQ(hint.width(), 0);
	EXPECT_EQ(hint.height(), 0);
}

TEST_F(FlowLayoutTest, SizeHintIncludesHorizontalSpacing)
{
	// Arrange — 4px horizontal spacing
	FlowLayout layout(nullptr, 0, 4, 0);
	layout.addWidget(makeItem(30, 20));
	layout.addWidget(makeItem(50, 20));

	// Act
	QSize hint = layout.sizeHint();

	// Assert — 30 + 4 + 50 = 84
	EXPECT_EQ(hint.width(), 84);
}

TEST_F(FlowLayoutTest, SizeHintHeightIsMaxItemHeight)
{
	// Arrange
	FlowLayout layout(nullptr, 0, 0, 0);
	layout.addWidget(makeItem(40, 10));
	layout.addWidget(makeItem(40, 35));
	layout.addWidget(makeItem(40, 20));

	// Act
	QSize hint = layout.sizeHint();

	// Assert
	EXPECT_EQ(hint.height(), 35);
}
