#include <gtest/gtest.h>
#include <QApplication>
#include <QLabel>

#include "QT/QLabelShim.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QLabelShimTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}
};

TEST_F(QLabelShimTest, SetText)
{
	// Arrange
	QLabel lbl;

	// Act
	qlabel_set_text(&lbl, "Status: OK");

	// Assert
	EXPECT_EQ(lbl.text(), QString("Status: OK"));
}

TEST_F(QLabelShimTest, SetTextOverwritesPreviousText)
{
	// Arrange
	QLabel lbl;
	lbl.setText("Old");

	// Act
	qlabel_set_text(&lbl, "New");

	// Assert
	EXPECT_EQ(lbl.text(), QString("New"));
}

TEST_F(QLabelShimTest, SetTextEmpty)
{
	// Arrange
	QLabel lbl;
	lbl.setText("Something");

	// Act
	qlabel_set_text(&lbl, "");

	// Assert
	EXPECT_EQ(lbl.text(), QString(""));
}

TEST_F(QLabelShimTest, SetTextNull_DoesNotCrash)
{
	// Arrange
	QLabel lbl;
	lbl.setText("Before");

	// Act + Assert
	EXPECT_NO_FATAL_FAILURE(qlabel_set_text(&lbl, nullptr));
	EXPECT_EQ(lbl.text(), QString("Before")); // unchanged
}

TEST_F(QLabelShimTest, NullPtr_DoesNotCrash)
{
	// Arrange + Act + Assert
	EXPECT_NO_FATAL_FAILURE(qlabel_set_text(nullptr, "text"));
}

TEST_F(QLabelShimTest, SetTextUtf8)
{
	// Arrange
	QLabel lbl;

	// Act
	qlabel_set_text(&lbl, reinterpret_cast<const char *>(u8"FPS: 60 — Recording"));

	// Assert
	EXPECT_EQ(lbl.text(), QString::fromUtf8(u8"FPS: 60 — Recording"));
}
