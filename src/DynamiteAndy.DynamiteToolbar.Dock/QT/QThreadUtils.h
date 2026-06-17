#pragma once
#include <QtCore/QCoreApplication>
#include <QtCore/QThread>
#include <QtCore/QMetaObject>
#include <type_traits>
#include <utility>

template<typename F> static auto runOnMainThread(F &&func) -> decltype(func())
{
	if (QThread::currentThread() == qApp->thread()) {
		return func();
	}

	if constexpr (!std::is_void_v<decltype(func())>) {
		decltype(func()) result;
		QMetaObject::invokeMethod(qApp, [&]() { result = func(); }, Qt::BlockingQueuedConnection);
		return result;
	} else {
		QMetaObject::invokeMethod(qApp, std::forward<F>(func), Qt::BlockingQueuedConnection);
	}
}
