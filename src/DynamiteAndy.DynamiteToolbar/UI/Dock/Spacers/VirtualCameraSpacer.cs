using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Spacers;

public class VirtualCameraSpacer : QWidget
{
    private readonly IPubSubService _pubSubService;

    public VirtualCameraSpacer(
        IQtControlContext qtControlContext,
        IPubSubService pubSubService,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Spacers.VirtualCameraSpacerPtr, qWidgetShim)
    {
        _pubSubService = pubSubService;

        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e) => Visible = !e.Settings.HideVirtualCamera;

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