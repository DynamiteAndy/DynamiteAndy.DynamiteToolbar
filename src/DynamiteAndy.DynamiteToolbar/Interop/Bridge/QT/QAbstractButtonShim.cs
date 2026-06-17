using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public sealed partial class QAbstractButtonShim : IQAbstractButtonShim
{
    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void qabstract_button_set_text(IntPtr ptr, string text);

    [LibraryImport(Constants.LibraryName)]
    private static partial void qabstract_button_set_checked(IntPtr ptr, int isChecked);

    [LibraryImport(Constants.LibraryName)]
    private static partial void qabstract_button_on_clicked(IntPtr ptr, IntPtr callback);

    public void SetText(IntPtr ptr, string text)
    {
        if (ptr != IntPtr.Zero)
        {
            qabstract_button_set_text(ptr, text);
        }
    }

    public void SetChecked(IntPtr ptr, bool isChecked)
    {
        if (ptr != IntPtr.Zero)
        {
            qabstract_button_set_checked(ptr, isChecked ? 1 : 0);
        }
    }

    public void OnClicked(IntPtr ptr, IntPtr callback)
    {
        if (ptr != IntPtr.Zero)
        {
            qabstract_button_on_clicked(ptr, callback);
        }
    }
}