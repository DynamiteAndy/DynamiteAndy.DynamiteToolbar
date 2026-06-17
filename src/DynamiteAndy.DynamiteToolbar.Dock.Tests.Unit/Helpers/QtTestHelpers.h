#pragma once
#include <QApplication>
#include <chrono>
#include <thread>

namespace QtTestHelpers {
inline void qWait(int ms)
{
	auto end = std::chrono::steady_clock::now() + std::chrono::milliseconds(ms);
	while (std::chrono::steady_clock::now() < end) {
		QApplication::processEvents();
		std::this_thread::sleep_for(std::chrono::milliseconds(1));
	}
}
} // namespace QtTestHelpers
