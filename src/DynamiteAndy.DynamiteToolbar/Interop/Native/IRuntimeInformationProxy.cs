namespace DynamiteAndy.DynamiteToolbar.Interop.Native;

public interface IRuntimeInformationProxy
{
    bool IsWindows();
    bool IsMacOS();
    bool IsLinux();
}