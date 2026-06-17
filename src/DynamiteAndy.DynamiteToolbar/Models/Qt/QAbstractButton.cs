using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Qt;

public class QAbstractButton(IntPtr ptr, IQAbstractButtonShim qAbstractButtonShim, IQWidgetShim qWidgetShim) : QWidget(ptr, qWidgetShim)
{
    private ClickedCallbackDelegate? _nativeDelegate;

    public event Action? Clicked;

    internal void RegisterCallback()
    {
        _nativeDelegate = (isChecked) => OnClicked(isChecked != 0);

        qAbstractButtonShim.OnClicked(
            Handle,
            Marshal.GetFunctionPointerForDelegate(_nativeDelegate));
    }

    protected virtual void OnClicked(bool isChecked) => Clicked?.Invoke();

    public virtual string? Text
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            if (value is not null)
            {
                qAbstractButtonShim.SetText(Handle, value);
            }
        }
    }

    public bool Checked
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            qAbstractButtonShim.SetChecked(Handle, value);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        qAbstractButtonShim.OnClicked(Handle, IntPtr.Zero);
        _nativeDelegate = null;

        base.Dispose(disposing);
    }
}