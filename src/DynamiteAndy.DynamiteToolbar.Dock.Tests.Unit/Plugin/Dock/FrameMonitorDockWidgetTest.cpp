#include <gtest/gtest.h>
#include <QApplication>

#include "../../Helpers/QtTestHelpers.h"
#include "Plugin/Dock/FrameMonitorDockWidget.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class FrameMonitorDockWidgetTest : public ::testing::Test {
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

TEST_F(FrameMonitorDockWidgetTest, ConstructsWithoutCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(FrameMonitorDockWidget w);
}

// ---------------------------------------------------------------------------
// populate — all label group pointers must be filled
// ---------------------------------------------------------------------------

TEST_F(FrameMonitorDockWidgetTest, Populate_SetsMonitorFramePtr)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_EQ(m.monitorFramePtr, static_cast<void *>(&w));
}

TEST_F(FrameMonitorDockWidgetTest, Populate_Streaming_AllPtrsNonNull)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_NE(m.streaming.containerHLayoutPtr, nullptr);
	EXPECT_NE(m.streaming.nameLblPtr, nullptr);
	EXPECT_NE(m.streaming.delimiterLblPtr, nullptr);
	EXPECT_NE(m.streaming.valueLblPtr, nullptr);
	EXPECT_NE(m.streaming.separatorLblPtr, nullptr);
}

TEST_F(FrameMonitorDockWidgetTest, Populate_Recording_AllPtrsNonNull)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_NE(m.recording.containerHLayoutPtr, nullptr);
	EXPECT_NE(m.recording.nameLblPtr, nullptr);
	EXPECT_NE(m.recording.delimiterLblPtr, nullptr);
	EXPECT_NE(m.recording.valueLblPtr, nullptr);
	EXPECT_NE(m.recording.separatorLblPtr, nullptr);
}

TEST_F(FrameMonitorDockWidgetTest, Populate_CurrentScene_AllPtrsNonNull)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_NE(m.currentScene.containerHLayoutPtr, nullptr);
	EXPECT_NE(m.currentScene.nameLblPtr, nullptr);
	EXPECT_NE(m.currentScene.delimiterLblPtr, nullptr);
	EXPECT_NE(m.currentScene.valueLblPtr, nullptr);
	EXPECT_NE(m.currentScene.separatorLblPtr, nullptr);
}

TEST_F(FrameMonitorDockWidgetTest, Populate_Cpu_AllPtrsNonNull)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_NE(m.cpu.containerHLayoutPtr, nullptr);
	EXPECT_NE(m.cpu.nameLblPtr, nullptr);
	EXPECT_NE(m.cpu.delimiterLblPtr, nullptr);
	EXPECT_NE(m.cpu.valueLblPtr, nullptr);
	EXPECT_NE(m.cpu.separatorLblPtr, nullptr);
}

TEST_F(FrameMonitorDockWidgetTest, Populate_Fps_AllPtrsNonNull)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_NE(m.fps.containerHLayoutPtr, nullptr);
	EXPECT_NE(m.fps.nameLblPtr, nullptr);
	EXPECT_NE(m.fps.delimiterLblPtr, nullptr);
	EXPECT_NE(m.fps.valueLblPtr, nullptr);
	EXPECT_NE(m.fps.separatorLblPtr, nullptr);
}

TEST_F(FrameMonitorDockWidgetTest, Populate_DroppedFrames_AllPtrsNonNull)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_NE(m.droppedFrames.containerHLayoutPtr, nullptr);
	EXPECT_NE(m.droppedFrames.nameLblPtr, nullptr);
	EXPECT_NE(m.droppedFrames.delimiterLblPtr, nullptr);
	EXPECT_NE(m.droppedFrames.valueLblPtr, nullptr);
	EXPECT_NE(m.droppedFrames.separatorLblPtr, nullptr);
}

TEST_F(FrameMonitorDockWidgetTest, Populate_Kbps_CorePtrsNonNull)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act
	DockFrameMonitor m = w.populate();

	// Assert
	EXPECT_NE(m.kbps.containerHLayoutPtr, nullptr);
	EXPECT_NE(m.kbps.nameLblPtr, nullptr);
	EXPECT_NE(m.kbps.delimiterLblPtr, nullptr);
	EXPECT_NE(m.kbps.valueLblPtr, nullptr);
	EXPECT_EQ(m.kbps.separatorLblPtr, nullptr);
}

TEST_F(FrameMonitorDockWidgetTest, Populate_CalledTwice_DoesNotCrash)
{
	// Arrange
	FrameMonitorDockWidget w;

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE({
		w.populate();
		w.populate();
	});
}

// ---------------------------------------------------------------------------
// hasHeightForWidth — delegates to FlowLayout
// ---------------------------------------------------------------------------

TEST_F(FrameMonitorDockWidgetTest, HasHeightForWidth_ReturnsTrue)
{
	// Arrange + Act
	FrameMonitorDockWidget w;

	// Assert
	EXPECT_TRUE(w.hasHeightForWidth());
}

// ---------------------------------------------------------------------------
// heightForWidth — delegates to layout
// ---------------------------------------------------------------------------

TEST_F(FrameMonitorDockWidgetTest, HeightForWidth_ReturnsPositive)
{
	// Arrange
	FrameMonitorDockWidget w;
	w.show();
	QtTestHelpers::qWait(50);

	// Act + Assert
	EXPECT_GT(w.heightForWidth(400), 0);
}

TEST_F(FrameMonitorDockWidgetTest, HeightForWidth_NarrowIsGreaterOrEqual)
{
	// Arrange
	FrameMonitorDockWidget w;
	w.show();
	QtTestHelpers::qWait(50);

	// Act + Assert
	EXPECT_GE(w.heightForWidth(100), w.heightForWidth(800));
}

// ---------------------------------------------------------------------------
// sizeHint — delegates to layout
// ---------------------------------------------------------------------------

TEST_F(FrameMonitorDockWidgetTest, SizeHint_ReturnsPositiveSize)
{
	// Arrange
	FrameMonitorDockWidget w;
	w.show();
	QtTestHelpers::qWait(50);

	// Act
	QSize hint = w.sizeHint();

	// Assert
	EXPECT_GT(hint.width(), 0);
	EXPECT_GT(hint.height(), 0);
}
