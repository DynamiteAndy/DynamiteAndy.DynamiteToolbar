using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;

public class FloatButton : QAbstractButton
{
    private readonly IQtControlContext _qtControlContext;
    private readonly IQDockWidgetShim _qDockWidgetShim;
    private readonly IPubSubService _pubSubService;

    private bool _visibilityFromSetting = true;
    private bool _isFloating;

    public FloatButton(
        IQtControlContext qtControlContext,
        ILocalizationService localizationService,
        IPubSubService pubSubService,
        IQDockWidgetShim qDockWidgetShim,
        IQAbstractButtonShim qAbstractButtonShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Buttons.FloatBtnPtr, qAbstractButtonShim, qWidgetShim)
    {
        _qtControlContext = qtControlContext;
        _qDockWidgetShim = qDockWidgetShim;
        _pubSubService = pubSubService;

        Text = localizationService.T("Toolbar.Button.Float");

        _pubSubService.Subscribe<TopLevelChangedEvent>(OnOnTopLevelChangedEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    protected override void OnClicked(bool isChecked) => _qDockWidgetShim.SetFloating(_qtControlContext.Dock!.Value.DockPtr, true);

    private void OnOnTopLevelChangedEvent(TopLevelChangedEvent e)
    {
        _isFloating = e.IsFloating;
        Checked = _isFloating;

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