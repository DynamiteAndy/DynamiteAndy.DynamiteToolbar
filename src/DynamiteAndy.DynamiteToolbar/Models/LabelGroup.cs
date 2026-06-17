using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Shim;

namespace DynamiteAndy.DynamiteToolbar.Models;

public class LabelGroup(DockFrameMonitorLabel group, IQLabelShim qLabelShim, IQWidgetShim qWidgetShim) : IDisposable
{
    public QLabel Name { get; } = new QLabel(group.NameLblPtr, qLabelShim, qWidgetShim);
    public QLabel Delimiter { get; } = new QLabel(group.DelimiterLblPtr, qLabelShim, qWidgetShim);
    public QLabel Value { get; } = new QLabel(group.ValueLblPtr, qLabelShim, qWidgetShim);
    public QLabel? Separator { get; } = group.SeparatorLblPtr != IntPtr.Zero
            ? new QLabel(group.SeparatorLblPtr, qLabelShim, qWidgetShim)
            : null;

    public event Action? VisibilityChanged;

    private bool _disposed;

    public bool Visible
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            Name.Visible = value;
            Delimiter.Visible = value;
            Value.Visible = value;
            Separator?.Visible = value;

            VisibilityChanged?.Invoke();
        }
    } = true;

    public void UpdateSeparator(bool isLastVisible)
    {
        if (Separator == null)
        {
            return;
        }

        Separator.Visible = Visible && !isLastVisible;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Name.Dispose();
            Delimiter.Dispose();
            Value.Dispose();
            Separator?.Dispose();
        }

        _disposed = true;
    }
}