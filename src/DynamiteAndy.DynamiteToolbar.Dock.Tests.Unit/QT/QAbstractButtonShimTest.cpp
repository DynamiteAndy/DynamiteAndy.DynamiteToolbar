#include <gtest/gtest.h>
#include <QApplication>
#include <QPushButton>

#include "QT/QAbstractButtonShim.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QAbstractButtonShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

TEST_F(QAbstractButtonShimTest, SetText)
{
	// Arrange
	QPushButton btn;

	// Act
	qabstract_button_set_text(&btn, "Hello");

	// Assert
	EXPECT_EQ(btn.text(), QString("Hello"));
}

TEST_F(QAbstractButtonShimTest, SetTextNull_DoesNotCrash)
{
	// Arrange
	QPushButton btn;
	btn.setText("Before");

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qabstract_button_set_text(&btn, nullptr));
	EXPECT_EQ(btn.text(), QString("Before")); // unchanged
}

TEST_F(QAbstractButtonShimTest, SetTextEmpty_ClearsText)
{
	// Arrange
	QPushButton btn;
	btn.setText("Something");

	// Act
	qabstract_button_set_text(&btn, "");

	// Assert
	EXPECT_EQ(btn.text(), QString(""));
}

TEST_F(QAbstractButtonShimTest, SetTextOverwritesPrevious)
{
	// Arrange
	QPushButton btn;
	qabstract_button_set_text(&btn, "First");

	// Act
	qabstract_button_set_text(&btn, "Second");

	// Assert
	EXPECT_EQ(btn.text(), QString("Second"));
}

TEST_F(QAbstractButtonShimTest, SetTextNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qabstract_button_set_text(nullptr, "text"));
}

TEST_F(QAbstractButtonShimTest, SetCheckedTrue)
{
	// Arrange
	QPushButton btn;
	btn.setCheckable(true);

	// Act
	qabstract_button_set_checked(&btn, 1);

	// Assert
	EXPECT_TRUE(btn.isChecked());
}

TEST_F(QAbstractButtonShimTest, SetCheckedFalse)
{
	// Arrange
	QPushButton btn;
	btn.setCheckable(true);
	btn.setChecked(true);

	// Act
	qabstract_button_set_checked(&btn, 0);

	// Assert
	EXPECT_FALSE(btn.isChecked());
}

TEST_F(QAbstractButtonShimTest, SetCheckedNonZeroTreatedAsTrue)
{
	// Arrange
	QPushButton btn;
	btn.setCheckable(true);

	// Act
	qabstract_button_set_checked(&btn, 42);

	// Assert
	EXPECT_TRUE(btn.isChecked());
}

TEST_F(QAbstractButtonShimTest, SetCheckedNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qabstract_button_set_checked(nullptr, 1));
}

static int g_callbackValue = -1;
static void onClicked(int v)
{
	g_callbackValue = v;
}

TEST_F(QAbstractButtonShimTest, OnClickedCallbackFired)
{
	// Arrange
	QPushButton btn;
	btn.setCheckable(true);
	g_callbackValue = -1;
	qabstract_button_on_clicked(&btn, onClicked);

	// Act + Assert — unchecked click gives 1, checked click gives 0
	btn.setChecked(false);
	btn.click();
	EXPECT_EQ(g_callbackValue, 1);

	btn.setChecked(true);
	btn.click();
	EXPECT_EQ(g_callbackValue, 0);
}

TEST_F(QAbstractButtonShimTest, OnClickedNullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qabstract_button_on_clicked(nullptr, onClicked));
}

TEST_F(QAbstractButtonShimTest, OnClickedNullCallback_DoesNotCrash)
{
	// Arrange
	QPushButton btn;

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qabstract_button_on_clicked(&btn, nullptr));
}

TEST_F(QAbstractButtonShimTest, ReconnectingCallbackReplacesOldOne)
{
	// Arrange
	QPushButton btn;
	btn.setCheckable(true);
	static int firstCount = 0;
	static int secondCount = 0;
	qabstract_button_on_clicked(&btn, [](int) { firstCount++; });

	// Act
	qabstract_button_on_clicked(&btn, [](int) { secondCount++; });
	btn.click();

	// Assert
	EXPECT_EQ(firstCount, 0);
	EXPECT_EQ(secondCount, 1);
}

TEST_F(QAbstractButtonShimTest, OnClicked_UncheckedButton_CallbackReceivesOne)
{
	// Arrange
	QPushButton btn;
	btn.setCheckable(true);
	btn.setChecked(false);
	g_callbackValue = -1;
	qabstract_button_on_clicked(&btn, [](int v) { g_callbackValue = v; });

	// Act
	btn.click();

	// Assert
	EXPECT_EQ(g_callbackValue, 1);
}

TEST_F(QAbstractButtonShimTest, OnClicked_CheckedButton_CallbackReceivesZero)
{
	// Arrange
	QPushButton btn;
	btn.setCheckable(true);
	btn.setChecked(true);
	g_callbackValue = -1;
	qabstract_button_on_clicked(&btn, onClicked);

	// Act
	btn.click();

	// Assert
	EXPECT_EQ(g_callbackValue, 0);
}
