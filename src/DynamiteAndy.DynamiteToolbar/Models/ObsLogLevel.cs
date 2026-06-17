using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.Models;

public enum ObsLogLevel : int
{
    Error = ObsBase.LOG_ERROR,
    Warning = ObsBase.LOG_WARNING,
    Info = ObsBase.LOG_INFO,
    Debug = ObsBase.LOG_DEBUG
}