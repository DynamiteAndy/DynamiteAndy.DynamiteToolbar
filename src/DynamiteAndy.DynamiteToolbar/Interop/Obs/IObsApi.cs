using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public interface IObsApi
{
    string? GetModuleBinaryPath();
    string? ModuleGetConfigPath(string fileName);
    IntPtr ModuleLoadLocale(string defaultLocale, string currentLocale);

    string? SourceGetName(IntPtr source);
    void SourceRelease(IntPtr source);

    int OutputGetFramesDropped(IntPtr output);
    int OutputGetTotalFrames(IntPtr output);
    ulong OutputGetTotalBytes(IntPtr output);
    void OutputRelease(IntPtr output);

    byte GetVideoInfo(ref obs_video_info ovi);
    double GetActiveFps();

    unsafe void Blog(int level, sbyte* message);
}