using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Native;

public sealed class NativeLibraryLoader(
    string libraryName,
    ILogger logger,
    IFileService fileService,
    IObsApi obsApi,
    IRuntimeInformationProxy runtimeInfo,
    INativeLibraryProxy nativeLibProxy) : INativeLibraryLoader, IDisposable
{
    private IntPtr _handle = IntPtr.Zero;
    private bool _disposed;

    public string LibraryName { get; } = libraryName;
    public bool IsLoaded => _handle != IntPtr.Zero;

    private string GetPlatformExtension() =>
        runtimeInfo.IsWindows() ? ".dll" :
        runtimeInfo.IsMacOS() ? ".dylib" :
                                 ".so";

    public void Load(string? overridePath = null)
    {
        if (IsLoaded)
        {
            return;
        }

        try
        {
            var resolvedPath = overridePath ?? ResolveDefaultPath();

            if (string.IsNullOrEmpty(resolvedPath) || !fileService.FileExists(resolvedPath))
            {
                logger.Error($"[{LibraryName}] Load failed: File not found at {resolvedPath}");
                return;
            }

            nativeLibProxy.SetDllImportResolver(Assembly.GetExecutingAssembly(), (name, _, _) =>
                (name == LibraryName && _handle != IntPtr.Zero) ? _handle : IntPtr.Zero);

            _handle = nativeLibProxy.Load(resolvedPath, Assembly.GetExecutingAssembly(), DllImportSearchPath.SafeDirectories);

            if (IsLoaded)
            {
                logger.Info($"[{LibraryName}] Loaded successfully (0x{_handle:X})");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[{LibraryName}] Exception: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed || !IsLoaded)
        {
            return;
        }

        nativeLibProxy.Free(_handle);
        _handle = IntPtr.Zero;
        _disposed = true;
    }

    private string? ResolveDefaultPath()
    {
        var binaryPath = obsApi.GetModuleBinaryPath();

        if (string.IsNullOrWhiteSpace(binaryPath))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(binaryPath);
        return directory != null
            ? Path.Combine(directory, $"{LibraryName}{GetPlatformExtension()}")
            : null;
    }
}