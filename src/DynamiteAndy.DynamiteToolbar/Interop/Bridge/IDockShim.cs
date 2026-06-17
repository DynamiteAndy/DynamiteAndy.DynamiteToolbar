using DynamiteAndy.DynamiteToolbar.Models.Shim;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge;

public interface IDockShim
{
    PluginDock CreateDock(string title);
    void DestroyDock();
}