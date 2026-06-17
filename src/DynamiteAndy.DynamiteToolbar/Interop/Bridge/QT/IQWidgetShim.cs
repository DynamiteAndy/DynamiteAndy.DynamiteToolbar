namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public interface IQWidgetShim
{
    void SetVisible(IntPtr ptr, bool visible);
    void SetTooltip(IntPtr ptr, string text);
}