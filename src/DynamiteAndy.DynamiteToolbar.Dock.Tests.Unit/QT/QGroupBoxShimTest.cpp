#include <gtest/gtest.h>
#include <QApplication>
#include <QGroupBox>

#include "QT/QGroupBoxShim.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QGroupBoxShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

TEST_F(QGroupBoxShimTest, SetTitle)
{
	// Arrange
	QGroupBox box;

	// Act
	qgroup_box_set_title(&box, "Buttons");

	// Assert
	EXPECT_EQ(box.title(), QString("Buttons"));
}

TEST_F(QGroupBoxShimTest, SetTitleOverwritesPrevious)
{
	// Arrange
	QGroupBox box("Old Title");

	// Act
	qgroup_box_set_title(&box, "New Title");

	// Assert
	EXPECT_EQ(box.title(), QString("New Title"));
}

TEST_F(QGroupBoxShimTest, SetTitleEmpty)
{
	// Arrange
	QGroupBox box("Something");

	// Act
	qgroup_box_set_title(&box, "");

	// Assert
	EXPECT_EQ(box.title(), QString(""));
}

TEST_F(QGroupBoxShimTest, SetTitleNull_DoesNotCrash)
{
	// Arrange
	QGroupBox box("Keep");

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qgroup_box_set_title(&box, nullptr));
	EXPECT_EQ(box.title(), QString("Keep"));
}

TEST_F(QGroupBoxShimTest, NullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qgroup_box_set_title(nullptr, "title"));
}
