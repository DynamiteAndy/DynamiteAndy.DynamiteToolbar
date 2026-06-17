using System.Reflection;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Native;

public class NativeLibraryProxy : INativeLibraryProxy
{
    public void SetDllImportResolver(Assembly assembly, DllImportResolver resolver)
        => NativeLibrary.SetDllImportResolver(assembly, resolver);

    public IntPtr Load(string libraryPath, Assembly assembly, DllImportSearchPath? searchPath)
        => NativeLibrary.Load(libraryPath, assembly, searchPath);

    public void Free(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            NativeLibrary.Free(handle);
        }
    }
}