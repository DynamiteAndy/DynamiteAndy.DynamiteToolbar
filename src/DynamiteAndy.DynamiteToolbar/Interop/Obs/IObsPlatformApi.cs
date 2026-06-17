namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public interface IObsPlatformApi
{
    IntPtr OSCpuUsageInfoStart();
    void OSCpuUsageInfoDestroy(IntPtr info);
    double OSCpuUsageInfoQuery(IntPtr info);
    ulong OSGetTimeNs();
}