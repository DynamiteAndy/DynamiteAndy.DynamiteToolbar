#include <gtest/gtest.h>
#include <QApplication>
#include <QAction>

#include "QT/QActionShim.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QActionShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

TEST_F(QActionShimTest, SetText)
{
	// Arrange
	QAction action;

	// Act
	qaction_set_text(&action, "My Action");

	// Assert
	EXPECT_EQ(action.text(), QString("My Action"));
}

TEST_F(QActionShimTest, SetTextNull_DoesNotCrash)
{
	// Arrange
	QAction action;
	action.setText("Before");

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qaction_set_text(&action, nullptr));
	EXPECT_EQ(action.text(), QString("Before"));
}

TEST_F(QActionShimTest, SetTextEmpty_ClearsText)
{
	// Arrange
	QAction action;
	action.setText("Something");

	// Act
	qaction_set_text(&action, "");

	// Assert
	EXPECT_EQ(action.text(), QString(""));
}

TEST_F(QActionShimTest, SetTextNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qaction_set_text(nullptr, "text"));
}

TEST_F(QActionShimTest, SetTextOverwritesPrevious)
{
	// Arrange
	QAction action;
	qaction_set_text(&action, "First");

	// Act
	qaction_set_text(&action, "Second");

	// Assert
	EXPECT_EQ(action.text(), QString("Second"));
}

TEST_F(QActionShimTest, SetEnabledTrue)
{
	// Arrange
	QAction action;
	action.setEnabled(false);

	// Act
	qaction_set_enabled(&action, 1);

	// Assert
	EXPECT_TRUE(action.isEnabled());
}

TEST_F(QActionShimTest, SetEnabledFalse)
{
	// Arrange
	QAction action;

	// Act
	qaction_set_enabled(&action, 0);

	// Assert
	EXPECT_FALSE(action.isEnabled());
}

TEST_F(QActionShimTest, SetEnabledNonZeroTreatedAsTrue)
{
	// Arrange
	QAction action;
	action.setEnabled(false);

	// Act
	qaction_set_enabled(&action, 99);

	// Assert
	EXPECT_TRUE(action.isEnabled());
}

TEST_F(QActionShimTest, SetEnabledNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qaction_set_enabled(nullptr, 1));
}

TEST_F(QActionShimTest, SetVisibleTrue)
{
	// Arrange
	QAction action;
	action.setVisible(false);

	// Act
	qaction_set_visible(&action, 1);

	// Assert
	EXPECT_TRUE(action.isVisible());
}

TEST_F(QActionShimTest, SetVisibleFalse)
{
	// Arrange
	QAction action;

	// Act
	qaction_set_visible(&action, 0);

	// Assert
	EXPECT_FALSE(action.isVisible());
}

TEST_F(QActionShimTest, SetVisibleNonZeroTreatedAsTrue)
{
	// Arrange
	QAction action;
	action.setVisible(false);

	// Act
	qaction_set_visible(&action, 7);

	// Assert
	EXPECT_TRUE(action.isVisible());
}

TEST_F(QActionShimTest, SetVisibleNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qaction_set_visible(nullptr, 1));
}

static int g_triggeredValue = -1;
static void onTriggered(int v)
{
	g_triggeredValue = v;
}

TEST_F(QActionShimTest, OnTriggeredCallbackFired)
{
	// Arrange
	QAction action;
	action.setCheckable(true);
	g_triggeredValue = -1;
	qaction_on_triggered(&action, onTriggered);

	// Act
	action.trigger();

	// Assert
	EXPECT_EQ(g_triggeredValue, 1);
}

TEST_F(QActionShimTest, OnTriggeredNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qaction_on_triggered(nullptr, onTriggered));
}

TEST_F(QActionShimTest, OnTriggeredNullCallback_DoesNotCrash)
{
	// Arrange
	QAction action;

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qaction_on_triggered(&action, nullptr));
}

TEST_F(QActionShimTest, ReconnectingCallbackReplacesOldOne)
{
	// Arrange
	QAction action;
	action.setCheckable(true);
	static int firstCount = 0;
	static int secondCount = 0;
	qaction_on_triggered(&action, [](int) { firstCount++; });

	// Act
	qaction_on_triggered(&action, [](int) { secondCount++; });
	action.trigger();

	// Assert
	EXPECT_EQ(firstCount, 0);
	EXPECT_EQ(secondCount, 1);
}

TEST_F(QActionShimTest, OnTriggered_CheckedAction_CallbackReceivesZero)
{
	// Arrange
	QAction action;
	action.setCheckable(true);
	action.setChecked(true);
	g_triggeredValue = -1;
	qaction_on_triggered(&action, onTriggered);

	// Act
	action.trigger();

	// Assert
	EXPECT_EQ(g_triggeredValue, 0);
}

TEST_F(QActionShimTest, OnTriggered_UncheckedAction_CallbackReceivesOne)
{
	// Arrange
	QAction action;
	action.setCheckable(true);
	action.setChecked(false);
	g_triggeredValue = -1;
	qaction_on_triggered(&action, onTriggered);

	// Act
	action.trigger();

	// Assert
	EXPECT_EQ(g_triggeredValue, 1);
}
