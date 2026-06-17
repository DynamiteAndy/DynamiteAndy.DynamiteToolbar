using ObsInterop;
using System.Runtime.InteropServices;
using System.Text;

namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public unsafe class ObsTextLookupApi : IObsTextLookupApi
{
    public string? GetString(IntPtr lookup, string identifier)
    {
        var lookupPtr = (text_lookup*)lookup;

        if (lookupPtr == null)
        {
            return null;
        }

        fixed (byte* keyPtr = Encoding.UTF8.GetBytes(identifier + "\0"))
        {
            sbyte* result = null;

            if (ObsTextLookup.text_lookup_getstr(lookupPtr, (sbyte*)keyPtr, &result) != 0 && result != null)
            {
                return Marshal.PtrToStringUTF8((IntPtr)result);
            }
        }

        return null;
    }

    public void Destroy(IntPtr lookup)
    {
        var lookupPtr = (text_lookup*)lookup;

        if (lookupPtr != null)
        {
            ObsTextLookup.text_lookup_destroy(lookupPtr);
        }
    }
}