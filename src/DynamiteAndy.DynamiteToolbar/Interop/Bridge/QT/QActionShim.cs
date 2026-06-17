using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public sealed partial class QActionShim : IQActionShim
{
    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void qaction_set_text(IntPtr ptr, string text);

    [LibraryImport(Constants.LibraryName)]
    private static partial void qaction_on_triggered(IntPtr ptr, IntPtr callback);

    public void SetText(IntPtr ptr, string text)
    {
        if (ptr != IntPtr.Zero)
        {
            qaction_set_text(ptr, text);
        }
    }

    public void OnTriggered(IntPtr ptr, IntPtr callback)
    {
        if (ptr != IntPtr.Zero)
        {
            qaction_on_triggered(ptr, callback);
        }
    }
}