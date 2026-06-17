namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public interface IObsModuleContext
{
    string ModuleName { get; }
    IntPtr ModulePointer { get; }
    void SetModuleContext(string name, IntPtr pointer);
}