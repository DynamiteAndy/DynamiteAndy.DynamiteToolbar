namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public interface IQDockWidgetShim
{
    void SetFloating(IntPtr ptr, bool floating);
    void OnTopLevelChanged(IntPtr ptr, IntPtr callback);
}