using DynamiteAndy.DynamiteToolbar.Models.Shim;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge;

public sealed partial class DockShim : IDockShim
{
    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void create_dock(string title, out PluginDock result);

    [LibraryImport(Constants.LibraryName)]
    private static partial void destroy_dock();

    public PluginDock CreateDock(string title)
    {
        create_dock(title, out var result);
        return result;
    }

    public void DestroyDock() => destroy_dock();
}