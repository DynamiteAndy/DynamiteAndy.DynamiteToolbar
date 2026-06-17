using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Spacers;

public class FloatSpacer : QWidget
{
    private readonly IPubSubService _pubSubService;

    private bool _visibilityFromSetting = true;
    private bool _isFloating;

    public FloatSpacer(
        IQtControlContext qtControlContext,
        IPubSubService pubSubService,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Spacers.FloatSpacerPtr, qWidgetShim)
    {
        _pubSubService = pubSubService;

        _pubSubService.Subscribe<TopLevelChangedEvent>(OnOnTopLevelChangedEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    private void OnOnTopLevelChangedEvent(TopLevelChangedEvent e)
    {
        _isFloating = e.IsFloating;

        UpdateVisibility();
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e)
    {
        _visibilityFromSetting = !e.Settings.HideFloat;

        UpdateVisibility();
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _pubSubService.Unsubscribe<TopLevelChangedEvent>(OnOnTopLevelChangedEvent);
        _pubSubService.Unsubscribe<SettingsChangedEvent>(OnSettingsChangedEvent);

        base.Dispose(disposing);
    }

    private void UpdateVisibility() => Visible = _visibilityFromSetting && !_isFloating;
}