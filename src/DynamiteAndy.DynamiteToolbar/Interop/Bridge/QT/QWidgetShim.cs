using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public sealed partial class QWidgetShim : IQWidgetShim
{
    [LibraryImport(Constants.LibraryName)]
    private static partial void qwidget_set_visible(IntPtr ptr, int visible);

    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void qwidget_set_tooltip(IntPtr ptr, string text);

    public void SetVisible(IntPtr ptr, bool visible)
    {
        if (ptr != IntPtr.Zero)
        {
            qwidget_set_visible(ptr, visible ? 1 : 0);
        }
    }

    public void SetTooltip(IntPtr ptr, string text)
    {
        if (ptr != IntPtr.Zero)
        {
            qwidget_set_tooltip(ptr, text);
        }
    }
}