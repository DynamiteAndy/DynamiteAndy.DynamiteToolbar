#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <QApplication>
#include <QAction>

#include "../../Mocks/MockObsFrontend.h"
#include "Plugin/Menu/ToolsMenuShim.h"

using ::testing::_;
using ::testing::Return;
using ::testing::StrEq;

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class ToolsMenuShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}

	void SetUp() override { g_mockObsFrontend = &m_mock; }
	void TearDown() override { g_mockObsFrontend = nullptr; }

	::testing::NiceMock<MockObsFrontend> m_mock;
};

TEST_F(ToolsMenuShimTest, AddToolsMenuQAction_CallsObsWithLabel)
{
	// Arrange
	QAction action;
	EXPECT_CALL(m_mock, add_tools_menu_qaction(StrEq("Settings"))).WillOnce(Return(static_cast<void *>(&action)));

	// Act
	void *result = add_tools_menu_qaction("Settings");

	// Assert
	EXPECT_EQ(result, static_cast<void *>(&action));
}

TEST_F(ToolsMenuShimTest, AddToolsMenuQAction_ReturnsNullWhenObsReturnsNull)
{
	// Arrange
	EXPECT_CALL(m_mock, add_tools_menu_qaction(_)).WillOnce(Return(nullptr));

	// Act
	void *result = add_tools_menu_qaction("Settings");

	// Assert
	EXPECT_EQ(result, nullptr);
}

TEST_F(ToolsMenuShimTest, AddToolsMenuQAction_EmptyLabel_DoesNotCrash)
{
	// Arrange
	EXPECT_CALL(m_mock, add_tools_menu_qaction(StrEq(""))).WillOnce(Return(nullptr));

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(add_tools_menu_qaction(""));
}

TEST_F(ToolsMenuShimTest, AddToolsMenuQAction_ReturnedPtrIsUsableAsQAction)
{
	// Arrange
	QAction action;
	action.setText("Settings");
	EXPECT_CALL(m_mock, add_tools_menu_qaction(_)).WillOnce(Return(static_cast<void *>(&action)));

	// Act
	void *result = add_tools_menu_qaction("Settings");
	auto *qa = static_cast<QAction *>(result);

	// Assert
	ASSERT_NE(qa, nullptr);
	EXPECT_EQ(qa->text(), QString("Settings"));
}
