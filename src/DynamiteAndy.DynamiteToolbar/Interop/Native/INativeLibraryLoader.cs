namespace DynamiteAndy.DynamiteToolbar.Interop.Native;

public interface INativeLibraryLoader
{
    string LibraryName { get; }
    bool IsLoaded { get; }
    void Load(string? overridePath = null);
}