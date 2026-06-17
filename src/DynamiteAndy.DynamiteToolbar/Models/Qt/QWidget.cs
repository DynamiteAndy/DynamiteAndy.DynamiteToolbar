using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

namespace DynamiteAndy.DynamiteToolbar.Models.Qt;

public class QWidget(IntPtr ptr, IQWidgetShim qWidgetShim) : QObject(ptr)
{
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
            qWidgetShim.SetVisible(Handle, value);
        }
    } = true;

    public string? Tooltip
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
                qWidgetShim.SetTooltip(Handle, value);
            }
        }
    }
}