#include <gtest/gtest.h>
#include <QApplication>
#include <QDockWidget>
#include <QMainWindow>

#include "../../Helpers/QtTestHelpers.h"
#include "Plugin/Dock/DockHeaderWidget.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class DockHeaderWidgetTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

// ---------------------------------------------------------------------------
// setTitle
// ---------------------------------------------------------------------------

TEST_F(DockHeaderWidgetTest, SetTitle_UpdatesLabel)
{
	// Arrange
	DockHeaderWidget header;

	// Act
	header.setTitle("My Dock");

	// Assert
	EXPECT_EQ(header.lblTitle->text(), QString("My Dock"));
}

TEST_F(DockHeaderWidgetTest, SetTitle_Empty)
{
	// Arrange
	DockHeaderWidget header;
	header.setTitle("Something");

	// Act
	header.setTitle("");

	// Assert
	EXPECT_EQ(header.lblTitle->text(), QString(""));
}

// ---------------------------------------------------------------------------
// setFloating
// ---------------------------------------------------------------------------

TEST_F(DockHeaderWidgetTest, SetFloatingTrue_ShowsTitleAndCloseButton)
{
	// Arrange
	DockHeaderWidget header;
	header.show();

	// Act
	header.setFloating(true);

	// Assert
	EXPECT_TRUE(header.lblTitle->isVisible());
	EXPECT_TRUE(header.btnClose->isVisible());
}

TEST_F(DockHeaderWidgetTest, SetFloatingFalse_HidesTitleAndCloseButton)
{
	// Arrange
	DockHeaderWidget header;
	header.show();
	header.setFloating(true);

	// Act
	header.setFloating(false);

	// Assert
	EXPECT_FALSE(header.lblTitle->isVisible());
	EXPECT_FALSE(header.btnClose->isVisible());
}

TEST_F(DockHeaderWidgetTest, SetFloatingTrue_SetsNonZeroHeight)
{
	// Arrange
	DockHeaderWidget header;

	// Act
	header.setFloating(true);

	// Assert
	EXPECT_GT(header.height(), 0);
}

TEST_F(DockHeaderWidgetTest, SetFloatingFalse_SetsZeroHeight)
{
	// Arrange
	DockHeaderWidget header;
	header.setFloating(true);

	// Act
	header.setFloating(false);

	// Assert
	EXPECT_EQ(header.height(), 0);
}

// ---------------------------------------------------------------------------
// installOn
// ---------------------------------------------------------------------------

TEST_F(DockHeaderWidgetTest, InstallOn_SetsTitleBarWidget)
{
	// Arrange
	QDockWidget wrapper;
	DockHeaderWidget *header = new DockHeaderWidget();

	// Act
	header->installOn(&wrapper);

	// Assert
	EXPECT_EQ(wrapper.titleBarWidget(), header);
}

TEST_F(DockHeaderWidgetTest, InstallOn_SyncsTitle)
{
	// Arrange
	QDockWidget wrapper;
	wrapper.setWindowTitle("Test Title");
	DockHeaderWidget *header = new DockHeaderWidget();

	// Act
	header->installOn(&wrapper);

	// Assert
	EXPECT_EQ(header->lblTitle->text(), QString("Test Title"));
}

TEST_F(DockHeaderWidgetTest, InstallOn_SyncsFloatingState)
{
	// Arrange
	QMainWindow mainWindow;
	QDockWidget wrapper;
	mainWindow.addDockWidget(Qt::LeftDockWidgetArea, &wrapper);
	mainWindow.show();
	wrapper.setFloating(true);
	QtTestHelpers::qWait(50);

	DockHeaderWidget *header = new DockHeaderWidget();
	header->show();

	// Act
	header->installOn(&wrapper);

	// Assert
	EXPECT_TRUE(header->lblTitle->isVisible());
	EXPECT_TRUE(header->btnClose->isVisible());
}

TEST_F(DockHeaderWidgetTest, InstallOn_CloseBtnClosesWrapper)
{
	// Arrange
	QDockWidget wrapper;
	wrapper.show();
	DockHeaderWidget *header = new DockHeaderWidget();
	header->installOn(&wrapper);
	wrapper.setFloating(true);

	// Act
	header->btnClose->click();
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_FALSE(wrapper.isVisible());
}

TEST_F(DockHeaderWidgetTest, InstallOn_TopLevelChangeUpdatesSyncedState)
{
	// Arrange
	QMainWindow mainWindow;
	QDockWidget wrapper;
	mainWindow.addDockWidget(Qt::LeftDockWidgetArea, &wrapper);
	mainWindow.show();
	wrapper.setFloating(false);
	DockHeaderWidget *header = new DockHeaderWidget();
	header->show();
	header->installOn(&wrapper);

	// Act
	wrapper.setFloating(true);
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_TRUE(header->lblTitle->isVisible());
}

TEST_F(DockHeaderWidgetTest, InstallOn_InnerAllowedAreasAppliedToWrapper)
{
	// Arrange
	QDockWidget wrapper;
	QDockWidget inner;
	inner.setAllowedAreas(Qt::LeftDockWidgetArea | Qt::RightDockWidgetArea);
	DockHeaderWidget *header = new DockHeaderWidget();

	// Act
	header->installOn(&wrapper, &inner);

	// Assert
	EXPECT_EQ(wrapper.allowedAreas(), Qt::LeftDockWidgetArea | Qt::RightDockWidgetArea);
}
