using System.Reflection;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Native;

public interface INativeLibraryProxy
{
    void SetDllImportResolver(Assembly assembly, DllImportResolver resolver);
    IntPtr Load(string libraryPath, Assembly assembly, DllImportSearchPath? searchPath);
    void Free(IntPtr handle);
}