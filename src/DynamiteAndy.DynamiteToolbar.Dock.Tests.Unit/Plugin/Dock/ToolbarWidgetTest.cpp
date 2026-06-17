#include <gtest/gtest.h>
#include <QApplication>
#include <QResizeEvent>

#include "../../Helpers/QtTestHelpers.h"
#include "Plugin/Dock/ToolbarWidget.h"
#include "Plugin/Types.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class ToolbarWidgetTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

TEST_F(ToolbarWidgetTest, ConstructsWithoutCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(ToolbarWidget w);
}

TEST_F(ToolbarWidgetTest, Populate_FillsPluginDockPtr)
{
	// Arrange
	ToolbarWidget w;
	PluginDock dock{};

	// Act
	w.populate(dock);

	// Assert
	EXPECT_NE(dock.monitor.monitorFramePtr, nullptr);
}

TEST_F(ToolbarWidgetTest, Populate_FillsAllButtonPtrs)
{
	// Arrange
	ToolbarWidget w;
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

TEST_F(ToolbarWidgetTest, Populate_FillsAllSpacerPtrs)
{
	// Arrange
	ToolbarWidget w;
	PluginDock dock{};

	// Act
	w.populate(dock);

	// Assert
	EXPECT_NE(dock.spacers.floatSpacerPtr, nullptr);
	EXPECT_NE(dock.spacers.studioModeSpacerPtr, nullptr);
	EXPECT_NE(dock.spacers.monitorSpacerPtr, nullptr);
	EXPECT_NE(dock.spacers.virtualCameraSpacerPtr, nullptr);
	EXPECT_NE(dock.spacers.replayBufferSpacerPtr, nullptr);
	EXPECT_NE(dock.spacers.recordSpacerPtr, nullptr);
}

TEST_F(ToolbarWidgetTest, Populate_CalledTwice_DoesNotCrash)
{
	// Arrange
	ToolbarWidget w;
	PluginDock dock{};

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE({
		w.populate(dock);
		w.populate(dock);
	});
}

TEST_F(ToolbarWidgetTest, HasHeightForWidth_WhenMonitorHasHfw)
{
	// Arrange + Act
	ToolbarWidget w;

	// Assert
	EXPECT_TRUE(w.hasHeightForWidth());
}

TEST_F(ToolbarWidgetTest, HeightForWidth_ReturnsPositiveValue)
{
	// Arrange
	ToolbarWidget w;
	w.show();
	w.resize(400, 100);
	QtTestHelpers::qWait(50);

	// Act + Assert
	if (w.hasHeightForWidth()) {
		EXPECT_GT(w.heightForWidth(400), 0);
	}
}

TEST_F(ToolbarWidgetTest, HeightForWidth_NarrowerWidthProducesGreaterOrEqualHeight)
{
	// Arrange
	ToolbarWidget w;
	w.show();
	w.resize(400, 100);
	QtTestHelpers::qWait(50);

	if (!w.hasHeightForWidth())
		GTEST_SKIP() << "hasHeightForWidth() returned false";

	// Act
	int hWide = w.heightForWidth(400);
	int hNarrow = w.heightForWidth(100);

	// Assert
	EXPECT_GE(hNarrow, hWide);
}

TEST_F(ToolbarWidgetTest, HeightForWidth_ZeroWidthDoesNotCrash)
{
	// Arrange
	ToolbarWidget w;

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE({
		if (w.hasHeightForWidth()) {
			(void)w.heightForWidth(0);
		}
	});
}

TEST_F(ToolbarWidgetTest, SizeHint_ReturnsPositiveSize)
{
	// Arrange
	ToolbarWidget w;
	w.show();
	QtTestHelpers::qWait(50);

	// Act
	QSize hint = w.sizeHint();

	// Assert
	EXPECT_GT(hint.width(), 0);
	EXPECT_GT(hint.height(), 0);
}

TEST_F(ToolbarWidgetTest, SizeHint_HeightConsistentWithHeightForWidth)
{
	// Arrange
	ToolbarWidget w;
	w.show();
	QtTestHelpers::qWait(50);

	if (!w.hasHeightForWidth()) {
		GTEST_SKIP() << "hasHeightForWidth() returned false";
	}

	// Act
	QSize hint = w.sizeHint();
	int expected = w.heightForWidth(hint.width());

	// Assert
	EXPECT_EQ(hint.height(), expected);
}

TEST_F(ToolbarWidgetTest, RefreshIcons_DoesNotCrash)
{
	// Arrange
	ToolbarWidget w;

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(w.refreshIcons());
}

TEST_F(ToolbarWidgetTest, RefreshIcons_CalledMultipleTimes_DoesNotCrash)
{
	// Arrange
	ToolbarWidget w;

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE({
		w.refreshIcons();
		w.refreshIcons();
		w.refreshIcons();
	});
}

TEST_F(ToolbarWidgetTest, PaletteChange_DoesNotCrash)
{
	// Arrange
	ToolbarWidget w;
	w.show();
	QPalette pal = w.palette();
	pal.setColor(QPalette::ButtonText, Qt::red);

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(w.setPalette(pal));
}

TEST_F(ToolbarWidgetTest, ResizeEvent_DoesNotCrash)
{
	// Arrange
	ToolbarWidget w;
	w.show();

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE({
		w.resize(600, 200);
		QtTestHelpers::qWait(50);
		w.resize(200, 200);
		QtTestHelpers::qWait(50);
	});
}

TEST_F(ToolbarWidgetTest, LayoutRequestEvent_DoesNotCrash)
{
	// Arrange
	ToolbarWidget w;
	w.show();
	QtTestHelpers::qWait(20);
	QEvent e(QEvent::LayoutRequest);

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(QApplication::sendEvent(&w, &e));
}

TEST_F(ToolbarWidgetTest, ResizeEvent_AppliesFixedHeight)
{
	// Arrange
	ToolbarWidget w;
	w.show();
	w.resize(400, 200);
	QtTestHelpers::qWait(50);

	if (!w.hasHeightForWidth()) {
		GTEST_SKIP() << "hasHeightForWidth() returned false";
	}

	// Act + Assert
	int fixedH = w.height();
	int expected = w.heightForWidth(400);
	EXPECT_EQ(fixedH, expected);
}
