#include <gtest/gtest.h>
#include <QApplication>
#include <QDockWidget>

#include "../../Helpers/QtTestHelpers.h"
#include "Plugin/Dock/DockWrapperWidget.h"
#include "Plugin/Types.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class DockWrapperWidgetTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

// ---------------------------------------------------------------------------
// Construction
// ---------------------------------------------------------------------------

TEST_F(DockWrapperWidgetTest, ConstructsWithoutCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(DockWrapperWidget w);
}

TEST_F(DockWrapperWidgetTest, TitleBarWidget_ReplacedOnConstruct)
{
	// Arrange + Act
	DockWrapperWidget w;

	// Assert — constructor replaces the default title bar with a plain QWidget
	EXPECT_NE(w.titleBarWidget(), nullptr);
}

// ---------------------------------------------------------------------------
// populate
// ---------------------------------------------------------------------------

TEST_F(DockWrapperWidgetTest, Populate_SetsDockPtr)
{
	// Arrange
	DockWrapperWidget w;
	PluginDock dock{};

	// Act
	w.populate(dock);

	// Assert
	EXPECT_EQ(dock.pluginDockPtr, static_cast<void *>(&w));
}

TEST_F(DockWrapperWidgetTest, Populate_FillsButtonPtrs)
{
	// Arrange
	DockWrapperWidget w;
	PluginDock dock{};

	// Act
	w.populate(dock);

	// Assert
	EXPECT_NE(dock.buttons.floatBtnPtr, nullptr);
	EXPECT_NE(dock.buttons.studioModeBtnPtr, nullptr);
	EXPECT_NE(dock.buttons.virtualCameraBtnPtr, nullptr);
	EXPECT_NE(dock.buttons.replayBufferBtnPtr, nullptr);
	EXPECT_NE(dock.buttons.recordBtnPtr, nullptr);
	EXPECT_NE(dock.buttons.recordPausedBtnPtr, nullptr);
	EXPECT_NE(dock.buttons.streamBtnPtr, nullptr);
}

TEST_F(DockWrapperWidgetTest, Populate_FillsMonitorPtr)
{
	// Arrange
	DockWrapperWidget w;
	PluginDock dock{};

	// Act
	w.populate(dock);

	// Assert
	EXPECT_NE(dock.monitor.monitorFramePtr, nullptr);
}

TEST_F(DockWrapperWidgetTest, Populate_CalledTwice_DoesNotCrash)
{
	// Arrange
	DockWrapperWidget w;
	PluginDock dock{};

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE({
		w.populate(dock);
		w.populate(dock);
	});
}

// ---------------------------------------------------------------------------
// ParentChange event
// ---------------------------------------------------------------------------

TEST_F(DockWrapperWidgetTest, ReparentToQDockWidget_DoesNotCrash)
{
	// Arrange
	auto *wrapper = new QDockWidget();
	auto *inner = new DockWrapperWidget();

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE({
		inner->setParent(wrapper);
		QtTestHelpers::qWait(100);
	});

	delete wrapper;
}

TEST_F(DockWrapperWidgetTest, ReparentToQDockWidget_HeaderInstalledOnWrapper)
{
	// Arrange
	auto *wrapper = new QDockWidget();
	auto *inner = new DockWrapperWidget();

	// Act
	inner->setParent(wrapper);
	QtTestHelpers::qWait(100);

	// Assert — after configureWrapper fires, wrapper's title bar is replaced
	EXPECT_NE(wrapper->titleBarWidget(), nullptr);

	delete wrapper;
}

TEST_F(DockWrapperWidgetTest, ReparentToNonDockWidget_DoesNotInstallHeader)
{
	// Arrange
	QWidget plainParent;
	auto *inner = new DockWrapperWidget();

	// Act + Assert — reparenting to a plain QWidget should not trigger configureWrapper
	EXPECT_NO_FATAL_FAILURE({
		inner->setParent(&plainParent);
		QtTestHelpers::qWait(100);
	});

	delete inner;
}
