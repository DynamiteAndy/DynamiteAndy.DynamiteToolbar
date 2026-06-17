#pragma once
#include <obs.h>
#include <string>

namespace Logger {
void Info(const std::string &message);
void Debug(const std::string &message);
void Warning(const std::string &message);
void Error(const std::string &message);
} // namespace Logger
