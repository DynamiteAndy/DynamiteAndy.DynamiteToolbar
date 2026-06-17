using ObsInterop;
using System.Text;

namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public unsafe class ObsDataApi : IObsDataApi
{
    public IntPtr Create() => (IntPtr)ObsData.obs_data_create();

    public IntPtr LoadFromJson(string path)
    {
        fixed (byte* pathPtr = Encoding.UTF8.GetBytes(path + "\0"))
        {
            var data = ObsData.obs_data_create_from_json_file((sbyte*)pathPtr);
            return (IntPtr)data;
        }
    }

    public bool SaveToJson(IntPtr data, string path)
    {
        fixed (byte* pathPtr = Encoding.UTF8.GetBytes(path + "\0"))
        {
            var result = ObsData.obs_data_save_json((obs_data*)data, (sbyte*)pathPtr);
            return result != 0;
        }
    }

    public void Release(IntPtr data)
    {
        if (data != IntPtr.Zero)
        {
            ObsData.obs_data_release((obs_data*)data);
        }
    }

    public bool GetBool(IntPtr data, string key)
    {
        fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key + "\0"))
        {
            return ObsData.obs_data_get_bool((obs_data*)data, (sbyte*)keyPtr) != 0;
        }
    }

    public void SetBool(IntPtr data, string key, bool value)
    {
        fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key + "\0"))
        {
            ObsData.obs_data_set_bool((obs_data*)data, (sbyte*)keyPtr, value ? (byte)1 : (byte)0);
        }
    }
}