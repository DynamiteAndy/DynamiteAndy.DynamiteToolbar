using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Models.Qt;

public class QDockWidget : QObject
{
    private readonly IQDockWidgetShim _qDockWidget;
    private TopLevelChangedCallbackDelegate? _nativeDelegate;

    public event Action? TopLevelChanged;

    public QDockWidget(IntPtr ptr, IQDockWidgetShim qDockWidget)
        : base(ptr)
    {
        _qDockWidget = qDockWidget;

        _nativeDelegate = (topLevel) => OnTopLevelChanged(topLevel != 0);
        _qDockWidget.OnTopLevelChanged(
            Handle,
            Marshal.GetFunctionPointerForDelegate(_nativeDelegate));
    }

    protected virtual void OnTopLevelChanged(bool topLevel) => TopLevelChanged?.Invoke();

    public bool Floating
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            _qDockWidget.SetFloating(Handle, value);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _qDockWidget.OnTopLevelChanged(Handle, IntPtr.Zero);
        _nativeDelegate = null;

        base.Dispose(disposing);
    }
}