#pragma once

#include <QtGui/QIcon>
#include <QtGui/QPixmap>
#include <QtGui/QPainter>
#include <QtGui/QPalette>
#include <QtCore/QFile>
#include <QtSvg/QSvgRenderer>

class QIconUtils {
public:
	/**
     * Generates a QIcon where "currentColor" in the SVG is replaced 
     * by the palette's text colors.
     */
	static QIcon createIcon(const QString &resourcePath, const QPalette &palette)
	{
		// We use ButtonText for the idle state and HighlightedText for the "active" state
		QColor normalColor = palette.color(QPalette::ButtonText);
		QColor activeColor = palette.color(QPalette::HighlightedText);

		QIcon icon;
		icon.addPixmap(render(resourcePath, normalColor), QIcon::Normal, QIcon::Off);
		icon.addPixmap(render(resourcePath, activeColor), QIcon::Normal, QIcon::On);
		return icon;
	}

private:
	static QPixmap render(const QString &path, const QColor &color)
	{
		QFile file(path);
		if (!file.open(QIODevice::ReadOnly))
			return QPixmap();

		QByteArray svgData = file.readAll();
		// The magic trick: swap the placeholder for the real Hex color
		svgData.replace("currentColor", color.name(QColor::HexRgb).toUtf8());

		QSvgRenderer renderer(svgData);
		QPixmap pixmap(16, 16);
		pixmap.fill(Qt::transparent);

		QPainter painter(&pixmap);
		renderer.render(&painter);
		return pixmap;
	}
};
