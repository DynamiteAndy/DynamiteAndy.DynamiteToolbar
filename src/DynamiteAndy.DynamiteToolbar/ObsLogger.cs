using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Services;
using System.Text;

namespace DynamiteAndy.DynamiteToolbar;

public sealed class ObsLogger(IObsModuleContext obsModuleContext, IObsApi obsApi) : ILogger
{
    private const bool DebugEnabled = true;

    public void Info(string message) => Log(ObsLogLevel.Info, message);
    public void Debug(string message) => Log(ObsLogLevel.Debug, message);
    public void Warning(string message) => Log(ObsLogLevel.Warning, message);
    public void Error(string message) => Log(ObsLogLevel.Error, message);

    private unsafe void Log(ObsLogLevel level, string message)
    {
        if (DebugEnabled && level == ObsLogLevel.Debug)
        {
            level = ObsLogLevel.Info;
        }

        var formatted = $"[{obsModuleContext.ModuleName}] {message.Replace("%", "%%")}";

        fixed (byte* ptr = Encoding.UTF8.GetBytes(formatted + "\0"))
        {
            obsApi.Blog((int)level, (sbyte*)ptr);
        }
    }
}