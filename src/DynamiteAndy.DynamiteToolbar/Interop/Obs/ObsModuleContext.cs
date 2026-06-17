namespace DynamiteAndy.DynamiteToolbar.Interop.Obs;

public class ObsModuleContext : IObsModuleContext
{
    public string ModuleName { get; private set; } = string.Empty;
    public IntPtr ModulePointer { get; private set; } = IntPtr.Zero;

    public void SetModuleContext(string name, IntPtr pointer)
    {
        if (ModulePointer != IntPtr.Zero)
        {
            throw new InvalidOperationException("ModulePointer has already been set.");
        }

        ModuleName = name;
        ModulePointer = pointer;
    }
}