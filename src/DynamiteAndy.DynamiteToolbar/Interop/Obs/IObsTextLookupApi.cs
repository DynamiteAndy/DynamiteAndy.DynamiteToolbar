namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public interface IObsTextLookupApi
{
    string? GetString(IntPtr lookup, string identifier);
    void Destroy(IntPtr lookup);
}