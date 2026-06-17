using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

namespace DynamiteAndy.DynamiteToolbar.Models.Qt;

public class QLabel(IntPtr ptr, IQLabelShim qLabelShim, IQWidgetShim qWidgetShim) : QWidget(ptr, qWidgetShim)
{
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
                qLabelShim.SetText(Handle, value);
            }
        }
    }
}