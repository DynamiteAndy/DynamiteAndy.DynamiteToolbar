namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public interface IQAbstractButtonShim
{
    void SetText(IntPtr ptr, string text);
    void SetChecked(IntPtr ptr, bool isChecked);
    void OnClicked(IntPtr ptr, IntPtr callback);
}