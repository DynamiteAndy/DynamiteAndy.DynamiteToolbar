using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Spacers;

public class RecordSpacer : QWidget
{
    private readonly IPubSubService _pubSubService;

    public RecordSpacer(
        IQtControlContext qtControlContext,
        IPubSubService pubSubService,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Spacers.RecordSpacerPtr, qWidgetShim)
    {
        _pubSubService = pubSubService;

        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e) => Visible = !e.Settings.HideStartRecording;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _pubSubService.Unsubscribe<SettingsChangedEvent>(OnSettingsChangedEvent);

        base.Dispose(disposing);
    }
}