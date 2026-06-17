using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public sealed partial class QLabelShim : IQLabelShim
{
    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void qlabel_set_text(IntPtr ptr, string text);

    public void SetText(IntPtr ptr, string text)
    {
        if (ptr != IntPtr.Zero)
        {
            qlabel_set_text(ptr, text);
        }
    }
}