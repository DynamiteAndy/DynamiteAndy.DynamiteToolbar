using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public sealed partial class QGroupBoxShim : IQGroupBoxShim
{
    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void qgroup_box_set_title(IntPtr ptr, string title);

    public void SetTitle(IntPtr ptr, string text)
    {
        if (ptr != IntPtr.Zero)
        {
            qgroup_box_set_title(ptr, text);
        }
    }
}