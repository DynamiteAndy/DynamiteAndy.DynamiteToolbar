namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public interface IQActionShim
{
    void SetText(IntPtr ptr, string text);
    void OnTriggered(IntPtr ptr, IntPtr callback);
}