#include "Logger.h"

namespace Logger {
const bool DebugEnabled = true;
const char *ModuleName = "DynamiteAndy.DynamiteToolbar.Shim";

void Log(int level, std::string message)
{
	if (DebugEnabled && level == LOG_DEBUG) {
		level = LOG_INFO;
	}

	size_t pos = 0;
	while ((pos = message.find('%', pos)) != std::string::npos) {
		message.replace(pos, 1, "%%");
		pos += 2;
	}

	blog(level, "[%s] %s", ModuleName, message.c_str());
}

void Info(const std::string &message)
{
	Log(LOG_INFO, message);
}
void Debug(const std::string &message)
{
	Log(LOG_DEBUG, message);
}
void Warning(const std::string &message)
{
	Log(LOG_WARNING, message);
}
void Error(const std::string &message)
{
	Log(LOG_ERROR, message);
}
} // namespace Logger
