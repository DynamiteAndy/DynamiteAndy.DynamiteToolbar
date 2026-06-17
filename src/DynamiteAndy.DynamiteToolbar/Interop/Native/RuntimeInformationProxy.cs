using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Native;

public class RuntimeInformationProxy : IRuntimeInformationProxy
{
    public bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}