using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public sealed partial class QDockWidgetShim : IQDockWidgetShim
{
    [LibraryImport(Constants.LibraryName)]
    private static partial void qdock_widget_set_floating(IntPtr ptr, int floating);

    [LibraryImport(Constants.LibraryName)]
    private static partial void qdock_widget_on_toplevelchanged(IntPtr ptr, IntPtr callback);

    public void SetFloating(IntPtr ptr, bool floating)
    {
        if (ptr != IntPtr.Zero)
        {
            qdock_widget_set_floating(ptr, floating ? 1 : 0);
        }
    }

    public void OnTopLevelChanged(IntPtr ptr, IntPtr callback)
    {
        if (ptr != IntPtr.Zero)
        {
            qdock_widget_on_toplevelchanged(ptr, callback);
        }
    }
}