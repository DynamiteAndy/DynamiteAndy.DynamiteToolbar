#pragma once
#include <QThread>
#include <QtCore/QMetaObject>
#include <QtCore/QString>
#include <string>
#include <functional>

template<typename T, typename Func> static void invoke_on_object(void *ptr, Func &&task)
{
	if (!ptr)
		return;

	auto *obj = static_cast<T *>(ptr);

	if (QThread::currentThread() == obj->thread()) {
		task(obj);
	} else {
		QMetaObject::invokeMethod(
			obj, [obj, task = std::forward<Func>(task)]() { task(obj); }, Qt::QueuedConnection);
	}
}

template<typename T> static void update_string(void *ptr, const char *text, std::function<void(T *, QString)> func)
{
	if (!text) {
		return;
	}

	std::string safeText(text);
	invoke_on_object<T>(ptr, [safeText, func](T *obj) { func(obj, QString::fromUtf8(safeText.c_str())); });
}

template<typename T> static void update_bool(void *ptr, int value, std::function<void(T *, bool)> func)
{
	bool bValue = (value != 0);
	invoke_on_object<T>(ptr, [bValue, func](T *obj) { func(obj, bValue); });
}

template<typename T, typename Signal, typename Adapter>
static void update_callback(void *ptr, Signal signal, Adapter &&adapter)
{
	invoke_on_object<T>(ptr, [signal, adapter = std::forward<Adapter>(adapter)](T *obj) {
		QObject::disconnect(obj, signal, nullptr, nullptr);
		QObject::connect(obj, signal, adapter);
	});
}
