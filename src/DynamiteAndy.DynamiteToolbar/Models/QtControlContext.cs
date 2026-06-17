using DynamiteAndy.DynamiteToolbar.Models.Shim;

namespace DynamiteAndy.DynamiteToolbar.Models;

public class QtControlContext : IQtControlContext
{
    public PluginDock? Dock { get; private set; } = null;

    public void SetQtControlContext(PluginDock dock)
    {
        if (Dock is not null)
        {
            throw new InvalidOperationException("QtControContext has already been set.");
        }

        Dock = dock;
    }
}