#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <QApplication>
#include <QWidget>

#include "../../Helpers/QtTestHelpers.h"
#include "../../Mocks/MockObsFrontend.h"
#include "Plugin/Dock/DockShim.h"

using ::testing::_;
using ::testing::NotNull;
using ::testing::StrNe;

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class DockShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}

	void SetUp() override
	{
		g_mockObsFrontend = &m_mock;
		destroy_dock();
	}

	void TearDown() override
	{
		destroy_dock();
		g_mockObsFrontend = nullptr;
	}

	::testing::NiceMock<MockObsFrontend> m_mock;
};

// ---------------------------------------------------------------------------
// create_dock
// ---------------------------------------------------------------------------

TEST_F(DockShimTest, CreateDock_CallsAddDockById)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(StrNe(""), StrNe(""), NotNull())).Times(1);
	PluginDock dock{};

	// Act
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);
}

TEST_F(DockShimTest, CreateDock_FillsOutPtr)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(1);
	PluginDock dock{};

	// Act
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_NE(dock.pluginDockPtr, nullptr);
}

TEST_F(DockShimTest, CreateDock_SetsWindowTitle)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(1);
	PluginDock dock{};

	// Act
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);

	// Assert
	auto *widget = static_cast<QWidget *>(dock.pluginDockPtr);
	ASSERT_NE(widget, nullptr);
	EXPECT_EQ(widget->windowTitle().toStdString(), std::string("My Toolbar"));
}

TEST_F(DockShimTest, CreateDock_NullOutPtr_DoesNotCrash)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(1);

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(create_dock("My Toolbar", nullptr));
	QtTestHelpers::qWait(50);
}

TEST_F(DockShimTest, CreateDock_CalledTwice_OnlyAddsOnce)
{
	// Arrange — second call should early-return; dock already exists
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(1);
	PluginDock dock{};

	// Act
	create_dock("My Toolbar", &dock);
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);
}

TEST_F(DockShimTest, CreateDock_CalledTwice_ReturnsSameDockPtr)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(1);
	PluginDock first{};
	create_dock("My Toolbar", &first);

	// Act
	PluginDock second{};
	create_dock("My Toolbar", &second);
	QtTestHelpers::qWait(50);

	// Assert
	EXPECT_EQ(first.pluginDockPtr, second.pluginDockPtr);
}

// ---------------------------------------------------------------------------
// destroy_dock
// ---------------------------------------------------------------------------

TEST_F(DockShimTest, DestroyDock_CallsRemoveDock)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(1);
	EXPECT_CALL(m_mock, remove_dock(StrNe(""))).Times(1);
	PluginDock dock{};
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);

	// Act
	destroy_dock();
}

TEST_F(DockShimTest, DestroyDock_WithoutCreate_DoesNotCrash)
{
	// Arrange
	EXPECT_CALL(m_mock, remove_dock(_)).Times(0);

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(destroy_dock());
}

TEST_F(DockShimTest, DestroyDock_CalledTwice_DoesNotCrash)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(1);
	EXPECT_CALL(m_mock, remove_dock(_)).Times(1);
	PluginDock dock{};
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);
	destroy_dock();

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(destroy_dock());
}

TEST_F(DockShimTest, CreateAfterDestroy_AddsDockAgain)
{
	// Arrange
	EXPECT_CALL(m_mock, add_dock_by_id(_, _, _)).Times(2);
	EXPECT_CALL(m_mock, remove_dock(_)).Times(2);
	PluginDock dock{};
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);
	destroy_dock();

	// Act
	create_dock("My Toolbar", &dock);
	QtTestHelpers::qWait(50);
}
