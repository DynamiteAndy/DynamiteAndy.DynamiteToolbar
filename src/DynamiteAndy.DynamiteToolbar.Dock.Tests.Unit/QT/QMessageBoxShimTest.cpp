#include <gtest/gtest.h>
#include <gmock/gmock.h>
#include <QAbstractButton>
#include <QApplication>
#include <QMainWindow>
#include <QMessageBox>
#include <QTimer>

#include "../Mocks/MockObsFrontend.h"
#include "QT/QMessageBoxShim.h"

using ::testing::Return;

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QMessageBoxShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}

		s_mainWindow = new QMainWindow();
	}

	static void TearDownTestSuite()
	{
		delete s_mainWindow;
		s_mainWindow = nullptr;
	}

	void SetUp() override
	{
		g_mockObsFrontend = &m_mock;
		ON_CALL(m_mock, get_main_window()).WillByDefault(Return(static_cast<void *>(s_mainWindow)));
	}

	void TearDown() override { g_mockObsFrontend = nullptr; }

	static void acceptNextDialog()
	{
		QTimer::singleShot(0, []() {
			for (QWidget *w : QApplication::topLevelWidgets()) {
				if (auto *mb = qobject_cast<QMessageBox *>(w)) {
					mb->button(QMessageBox::Yes) ? mb->button(QMessageBox::Yes)->click()
								     : mb->accept();
					return;
				}
			}
		});
	}

	static void rejectNextDialog()
	{
		QTimer::singleShot(0, []() {
			for (QWidget *w : QApplication::topLevelWidgets()) {
				if (auto *mb = qobject_cast<QMessageBox *>(w)) {
					mb->button(QMessageBox::No) ? mb->button(QMessageBox::No)->click()
								    : mb->reject();
					return;
				}
			}
		});
	}

	::testing::NiceMock<MockObsFrontend> m_mock;
	static QMainWindow *s_mainWindow;
};

QMainWindow *QMessageBoxShimTest::s_mainWindow = nullptr;

TEST_F(QMessageBoxShimTest, ShowConfirmation_YesReturnsTrue)
{
	// Arrange
	acceptNextDialog();

	// Act
	bool result = show_confirmation("Confirm", "Are you sure?");

	// Assert
	EXPECT_TRUE(result);
}

TEST_F(QMessageBoxShimTest, ShowConfirmation_NoReturnsFalse)
{
	// Arrange
	rejectNextDialog();

	// Act
	bool result = show_confirmation("Confirm", "Are you sure?");

	// Assert
	EXPECT_FALSE(result);
}

TEST_F(QMessageBoxShimTest, ShowConfirmation_UsesMainWindowFromObs)
{
	// Arrange
	EXPECT_CALL(m_mock, get_main_window()).Times(1);
	acceptNextDialog();

	// Act
	show_confirmation("Title", "Text");
}

TEST_F(QMessageBoxShimTest, ShowConfirmation_NullMainWindow_DoesNotCrash)
{
	// Arrange
	ON_CALL(m_mock, get_main_window()).WillByDefault(Return(nullptr));
	acceptNextDialog();

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(show_confirmation("Title", "Text"));
}

TEST_F(QMessageBoxShimTest, ShowConfirmation_EmptyStrings_DoesNotCrash)
{
	// Arrange
	acceptNextDialog();

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(show_confirmation("", ""));
}
