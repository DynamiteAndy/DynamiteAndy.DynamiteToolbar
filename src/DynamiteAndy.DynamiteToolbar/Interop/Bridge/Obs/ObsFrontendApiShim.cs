using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Obs;

public static partial class ObsFrontendApiShim
{
    [LibraryImport(Constants.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr add_tools_menu_qaction(string actionLabel);
}