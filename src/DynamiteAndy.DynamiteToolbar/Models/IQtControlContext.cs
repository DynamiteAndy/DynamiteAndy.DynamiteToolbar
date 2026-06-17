using DynamiteAndy.DynamiteToolbar.Models.Shim;

namespace DynamiteAndy.DynamiteToolbar.Models;

public interface IQtControlContext
{
    PluginDock? Dock { get; }

    void SetQtControlContext(PluginDock dock);
}