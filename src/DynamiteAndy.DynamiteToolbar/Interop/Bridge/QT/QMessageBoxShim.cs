using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public sealed partial class QMessageBoxShim : IQMessageBoxShim
{
    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool show_confirmation(string title, string text);

    public bool ShowConfirmation(string title, string text) => show_confirmation(title, text);
}