namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

public interface IQMessageBoxShim
{
    bool ShowConfirmation(string title, string text);
}