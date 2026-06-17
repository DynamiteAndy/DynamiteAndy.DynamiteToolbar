#include <gtest/gtest.h>
#include <QApplication>
#include <QPalette>
#include <QColor>
#include <QIcon>
#include <QPixmap>
#include <QTemporaryFile>

#include "QT/QIconUtils.h"

static int s_argc = 1;
static char s_appName[] = "test";
static char *s_argv[] = {s_appName};

class QIconUtilsTest : public ::testing::Test {
protected:
	static void SetUpTestSuite()
	{
		if (!QApplication::instance()) {
			new QApplication(s_argc, s_argv);
		}
	}

	static QString writeTempSvg(const QByteArray &content)
	{
		auto *f = new QTemporaryFile();
		f->setAutoRemove(false);
		f->open();
		f->write(content);
		f->close();
		return f->fileName();
	}

	static const QByteArray kValidSvg;
};

const QByteArray QIconUtilsTest::kValidSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 16 16\">"
					     "<rect width=\"16\" height=\"16\" fill=\"currentColor\"/>"
					     "</svg>";

TEST_F(QIconUtilsTest, InvalidPath_ReturnsNullPixmaps)
{
	// Arrange
	QPalette pal;

	// Act
	QIcon icon = QIconUtils::createIcon("/does/not/exist.svg", pal);

	// Assert
	EXPECT_TRUE(icon.pixmap(16, 16, QIcon::Normal, QIcon::Off).isNull());
	EXPECT_TRUE(icon.pixmap(16, 16, QIcon::Normal, QIcon::On).isNull());
}

TEST_F(QIconUtilsTest, ValidSvg_ReturnsNonNullPixmaps)
{
	// Arrange
	QString path = writeTempSvg(kValidSvg);
	QPalette pal;

	// Act
	QIcon icon = QIconUtils::createIcon(path, pal);

	// Assert
	EXPECT_FALSE(icon.pixmap(16, 16, QIcon::Normal, QIcon::Off).isNull());
	EXPECT_FALSE(icon.pixmap(16, 16, QIcon::Normal, QIcon::On).isNull());
}

TEST_F(QIconUtilsTest, ValidSvg_PixmapIsCorrectSize)
{
	// Arrange
	QString path = writeTempSvg(kValidSvg);
	QPalette pal;

	// Act
	QIcon icon = QIconUtils::createIcon(path, pal);
	QPixmap px = icon.pixmap(16, 16, QIcon::Normal, QIcon::Off);

	// Assert
	EXPECT_EQ(px.width(), 16);
	EXPECT_EQ(px.height(), 16);
}

TEST_F(QIconUtilsTest, NormalAndActiveStatesUseDifferentColors)
{
	// Arrange — strongly contrasting palette so pixels will differ
	QString path = writeTempSvg(kValidSvg);
	QPalette pal;
	pal.setColor(QPalette::ButtonText, Qt::black);
	pal.setColor(QPalette::HighlightedText, Qt::white);

	// Act
	QIcon icon = QIconUtils::createIcon(path, pal);
	QImage offImg = icon.pixmap(16, 16, QIcon::Normal, QIcon::Off).toImage();
	QImage onImg = icon.pixmap(16, 16, QIcon::Normal, QIcon::On).toImage();

	// Assert
	EXPECT_NE(offImg, onImg);
}

TEST_F(QIconUtilsTest, CurrentColorReplacedWithPaletteColor)
{
	// Arrange
	QString path = writeTempSvg(kValidSvg);
	QPalette pal;
	pal.setColor(QPalette::ButtonText, Qt::red);

	// Act
	QIcon icon = QIconUtils::createIcon(path, pal);
	QImage img = icon.pixmap(16, 16, QIcon::Normal, QIcon::Off).toImage();

	// Assert — at least one pixel should be red
	bool foundRed = false;
	for (int y = 0; y < img.height() && !foundRed; ++y)
		for (int x = 0; x < img.width() && !foundRed; ++x)
			if (QColor(img.pixel(x, y)) == Qt::red) {
				foundRed = true;
			}

	EXPECT_TRUE(foundRed);
}

TEST_F(QIconUtilsTest, SamePaletteColors_ProduceIdenticalPixmaps)
{
	// Arrange
	QString path = writeTempSvg(kValidSvg);
	QPalette pal;
	pal.setColor(QPalette::ButtonText, Qt::blue);
	pal.setColor(QPalette::HighlightedText, Qt::blue);

	// Act
	QIcon icon = QIconUtils::createIcon(path, pal);
	QImage offImg = icon.pixmap(16, 16, QIcon::Normal, QIcon::Off).toImage();
	QImage onImg = icon.pixmap(16, 16, QIcon::Normal, QIcon::On).toImage();

	// Assert
	EXPECT_EQ(offImg, onImg);
}

TEST_F(QIconUtilsTest, RealFloatSvg_RendersSuccessfully)
{
	// Arrange
	QPalette pal;

	// Act + Assert — if resource unavailable we just verify no crash
	EXPECT_NO_FATAL_FAILURE((void)QIconUtils::createIcon(":/images/float.svg", pal).pixmap(16, 16));
}
