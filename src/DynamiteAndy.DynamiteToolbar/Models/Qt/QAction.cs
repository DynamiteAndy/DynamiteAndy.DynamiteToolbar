using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Qt;

public class QAction(IntPtr ptr, IQActionShim qActionShim) : QObject(ptr)
{
    private TriggeredCallbackDelegate? _nativeDelegate;

    public event Action? Triggered;

    internal void RegisterCallback()
    {
        _nativeDelegate = (isChecked) => OnTriggered(isChecked != 0);

        qActionShim.OnTriggered(
            Handle,
            Marshal.GetFunctionPointerForDelegate(_nativeDelegate));
    }

    protected virtual void OnTriggered(bool isChecked) => Triggered?.Invoke();

    public string? Text
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
                qActionShim.SetText(Handle, value);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        qActionShim.OnTriggered(Handle, IntPtr.Zero);
        _nativeDelegate = null;

        base.Dispose(disposing);
    }
}