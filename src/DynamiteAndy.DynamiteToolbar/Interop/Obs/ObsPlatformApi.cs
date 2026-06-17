using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public unsafe class ObsPlatformApi : IObsPlatformApi
{
    public IntPtr OSCpuUsageInfoStart() => (IntPtr)ObsPlatform.os_cpu_usage_info_start();

    public void OSCpuUsageInfoDestroy(IntPtr info) => ObsPlatform.os_cpu_usage_info_destroy((os_cpu_usage_info*)info);

    public double OSCpuUsageInfoQuery(IntPtr info) => ObsPlatform.os_cpu_usage_info_query((os_cpu_usage_info*)info);

    public ulong OSGetTimeNs() => ObsPlatform.os_gettime_ns();
}