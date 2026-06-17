using ObsInterop;
using System.Runtime.InteropServices;
using System.Text;

namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public unsafe class ObsApi(IObsModuleContext obsModuleContext) : IObsApi
{
    public string? GetModuleBinaryPath()
    {
        var ptr = ObsInterop.Obs.obs_get_module_binary_path((obs_module*)obsModuleContext.ModulePointer);
        return ptr == null ? null : Marshal.PtrToStringUTF8((IntPtr)ptr);
    }

    public string? ModuleGetConfigPath(string fileName)
    {
        fixed (byte* filePtr = Encoding.UTF8.GetBytes(fileName + "\0"))
        {
            var ptr = ObsInterop.Obs.obs_module_get_config_path(
                (obs_module*)obsModuleContext.ModulePointer,
                (sbyte*)filePtr);

            if (ptr == null)
            {
                return null;
            }

            var result = Marshal.PtrToStringUTF8((IntPtr)ptr);
            ObsBmem.bfree(ptr);
            return result;
        }
    }

    public IntPtr ModuleLoadLocale(string defaultLocale, string currentLocale)
    {
        fixed (byte* defPtr = Encoding.UTF8.GetBytes(defaultLocale + "\0"))
        fixed (byte* currPtr = Encoding.UTF8.GetBytes(currentLocale + "\0"))
        {
            var pointer = ObsInterop.Obs.obs_module_load_locale(
                (obs_module*)obsModuleContext.ModulePointer,
                (sbyte*)defPtr,
                (sbyte*)currPtr);

            return (IntPtr)pointer;
        }
    }

    public string? SourceGetName(IntPtr source) => Marshal.PtrToStringUTF8((IntPtr)ObsInterop.Obs.obs_source_get_name((obs_source*)source));

    public void SourceRelease(IntPtr source) => ObsInterop.Obs.obs_source_release((obs_source*)source);

    public int OutputGetFramesDropped(IntPtr output) => ObsInterop.Obs.obs_output_get_frames_dropped((obs_output*)output);

    public int OutputGetTotalFrames(IntPtr output) => ObsInterop.Obs.obs_output_get_total_frames((obs_output*)output);

    public ulong OutputGetTotalBytes(IntPtr output) => ObsInterop.Obs.obs_output_get_total_bytes((obs_output*)output);

    public void OutputRelease(IntPtr output) => ObsInterop.Obs.obs_output_release((obs_output*)output);

    public byte GetVideoInfo(ref obs_video_info ovi)
    {
        fixed (obs_video_info* pOvi = &ovi)
        {
            return ObsInterop.Obs.obs_get_video_info(pOvi);
        }
    }

    public double GetActiveFps() => ObsInterop.Obs.obs_get_active_fps();

    public void Blog(int level, sbyte* message) => ObsBase.blog(level, message);
}