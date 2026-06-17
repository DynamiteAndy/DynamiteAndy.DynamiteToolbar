#include <gtest/gtest.h>
#include <QApplication>
#include <QWidget>

#include "QT/QWidgetShim.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QWidgetShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

TEST_F(QWidgetShimTest, SetVisibleTrue)
{
	// Arrange
	QWidget w;
	w.hide();

	// Act
	qwidget_set_visible(&w, 1);

	// Assert
	EXPECT_TRUE(w.isVisible());
}

TEST_F(QWidgetShimTest, SetVisibleFalse)
{
	// Arrange
	QWidget w;
	w.show();

	// Act
	qwidget_set_visible(&w, 0);

	// Assert
	EXPECT_FALSE(w.isVisible());
}

TEST_F(QWidgetShimTest, SetVisibleNonZeroTreatedAsTrue)
{
	// Arrange
	QWidget w;
	w.hide();

	// Act
	qwidget_set_visible(&w, 99);

	// Assert
	EXPECT_TRUE(w.isVisible());
}

TEST_F(QWidgetShimTest, SetEnabledTrue)
{
	// Arrange
	QWidget w;
	w.setEnabled(false);

	// Act
	qwidget_set_enabled(&w, 1);

	// Assert
	EXPECT_TRUE(w.isEnabled());
}

TEST_F(QWidgetShimTest, SetEnabledFalse)
{
	// Arrange
	QWidget w;

	// Act
	qwidget_set_enabled(&w, 0);

	// Assert
	EXPECT_FALSE(w.isEnabled());
}

TEST_F(QWidgetShimTest, SetEnabledNonZeroTreatedAsTrue)
{
	// Arrange
	QWidget w;
	w.setEnabled(false);

	// Act
	qwidget_set_enabled(&w, 42);

	// Assert
	EXPECT_TRUE(w.isEnabled());
}

TEST_F(QWidgetShimTest, SetTooltip)
{
	// Arrange
	QWidget w;

	// Act
	qwidget_set_tooltip(&w, "Hover me");

	// Assert
	EXPECT_EQ(w.toolTip(), QString("Hover me"));
}

TEST_F(QWidgetShimTest, SetTooltipNull_DoesNotCrash)
{
	// Arrange
	QWidget w;
	w.setToolTip("Before");

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qwidget_set_tooltip(&w, nullptr));
	EXPECT_EQ(w.toolTip(), QString("Before"));
}

TEST_F(QWidgetShimTest, SetTooltipEmpty_ClearsTooltip)
{
	// Arrange
	QWidget w;
	w.setToolTip("Old tip");

	// Act
	qwidget_set_tooltip(&w, "");

	// Assert
	EXPECT_EQ(w.toolTip(), QString(""));
}

TEST_F(QWidgetShimTest, SetTooltipOverwritesPrevious)
{
	// Arrange
	QWidget w;
	qwidget_set_tooltip(&w, "First");

	// Act
	qwidget_set_tooltip(&w, "Second");

	// Assert
	EXPECT_EQ(w.toolTip(), QString("Second"));
}

TEST_F(QWidgetShimTest, SetVisibleNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qwidget_set_visible(nullptr, 1));
}

TEST_F(QWidgetShimTest, SetEnabledNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qwidget_set_enabled(nullptr, 1));
}

TEST_F(QWidgetShimTest, SetTooltipNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qwidget_set_tooltip(nullptr, "x"));
}
