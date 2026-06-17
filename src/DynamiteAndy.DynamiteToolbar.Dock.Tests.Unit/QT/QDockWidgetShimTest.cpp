#include <gtest/gtest.h>
#include <QApplication>
#include <QDockWidget>
#include <QMainWindow>

#include "../Helpers/QtTestHelpers.h"
#include "QT/QDockWidgetShim.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QDockWidgetShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

TEST_F(QDockWidgetShimTest, SetFloatingTrue_NoParent)
{
	// Arrange
	QDockWidget dock;
	dock.setFloating(false);

	// Act
	qdock_widget_set_floating(&dock, 1);

	// Assert
	EXPECT_TRUE(dock.isFloating());
}

TEST_F(QDockWidgetShimTest, SetFloatingFalse_NoParent_RequiresMainWindow)
{
	// Arrange
	QDockWidget dock;
	dock.setFloating(true);

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qdock_widget_set_floating(&dock, 0));
}

TEST_F(QDockWidgetShimTest, SetFloating_NoOp_WhenAlreadyInDesiredState)
{
	// Arrange
	QDockWidget dock;
	dock.setFloating(true);
	bool stateChanged = false;
	QObject::connect(&dock, &QDockWidget::topLevelChanged, [&](bool) { stateChanged = true; });

	// Act
	qdock_widget_set_floating(&dock, 1);
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_FALSE(stateChanged);
}

TEST_F(QDockWidgetShimTest, SetFloating_TargetsWrapper_WhenParentIsDockWidget)
{
	// Arrange
	QDockWidget wrapper;
	QDockWidget inner;
	inner.setParent(&wrapper);
	wrapper.setFloating(false);

	// Act
	qdock_widget_set_floating(&inner, 1);

	// Assert
	EXPECT_TRUE(wrapper.isFloating());
	EXPECT_FALSE(inner.isFloating());
}

TEST_F(QDockWidgetShimTest, SetFloatingNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qdock_widget_set_floating(nullptr, 1));
}

TEST_F(QDockWidgetShimTest, OnTopLevelChanged_NoWrapper_DoesNotCrash)
{
	// Arrange
	QDockWidget dock;
	static int callCount = 0;
	callCount = 0;

	// Act
	qdock_widget_on_toplevelchanged(&dock, [](int) { callCount++; });
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_EQ(callCount, 0);
}

TEST_F(QDockWidgetShimTest, OnTopLevelChanged_FiresImmediatelyWithCurrentState)
{
	// Arrange
	QDockWidget wrapper;
	QDockWidget inner;
	inner.setParent(&wrapper);
	wrapper.setFloating(true);
	static int lastValue = -1;
	lastValue = -1;

	// Act
	qdock_widget_on_toplevelchanged(&inner, [](int v) { lastValue = v; });
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_EQ(lastValue, 1);
}

TEST_F(QDockWidgetShimTest, OnTopLevelChanged_CallbackFiredOnSubsequentChange)
{
	// Arrange
	QDockWidget wrapper;
	QDockWidget inner;
	inner.setParent(&wrapper);
	wrapper.setFloating(false);
	static int lastValue = -1;
	lastValue = -1;
	qdock_widget_on_toplevelchanged(&inner, [](int v) { lastValue = v; });
	QtTestHelpers::qWait(50);

	// Act
	wrapper.setFloating(true);
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_EQ(lastValue, 1);
}

TEST_F(QDockWidgetShimTest, OnTopLevelChangedNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qdock_widget_on_toplevelchanged(nullptr, [](int) {}));
}

TEST_F(QDockWidgetShimTest, OnTopLevelChangedNullCallback_DoesNotCrash)
{
	// Arrange
	QDockWidget dock;

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qdock_widget_on_toplevelchanged(&dock, nullptr));
}
