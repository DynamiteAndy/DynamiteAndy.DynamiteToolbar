using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.UI.Dialog;
using DynamiteAndy.DynamiteToolbar.UI.Menu.Items;

namespace DynamiteAndy.DynamiteToolbar.UI.Menu;

public class ToolsMenu(
    IObsFrontendApi obsFrontendApi,
    ILocalizationService localizationService,
    IQActionShim qActionShim,
    SettingsDialog settingsDialog) : IDisposable
{
    private QAction? _settings;

    public void Register()
    {
        var actionPtr = obsFrontendApi.AddToolsMenuQAction(localizationService.T("Toolbar.Menu.Tools.Settings"));

        _settings = new DynamiteToolbarSettingsQAction(actionPtr, qActionShim, settingsDialog);
        _settings.RegisterCallback();
    }

    public void Dispose()
    {
        _settings?.Dispose();
        _settings = null;

        GC.SuppressFinalize(this);
    }
}