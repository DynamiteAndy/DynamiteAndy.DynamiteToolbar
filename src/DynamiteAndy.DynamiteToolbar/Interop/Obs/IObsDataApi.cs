namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public interface IObsDataApi
{
    IntPtr Create();
    IntPtr LoadFromJson(string path);
    bool SaveToJson(IntPtr data, string path);
    void Release(IntPtr data);


    bool GetBool(IntPtr data, string key);
    void SetBool(IntPtr data, string key, bool value);
}