using DynamiteAndy.DynamiteToolbar.Factories;
using DynamiteAndy.DynamiteToolbar.Interop.Bridge;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.Settings;
using DynamiteAndy.DynamiteToolbar.UI.Dock;
using DynamiteAndy.DynamiteToolbar.UI.Menu;
using ObsInterop;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar;

public class Plugin(
    IQtControlContext qtControlContext,
    IQtFactory qtFactory,
    IObsFrontendApi obsAdapter,
    IDockShim dockShim,
    ILocalizationService localizationService,
    IPubSubService pubSubService,
    IHeartbeatService heartbeatService,
    ISettingsService settingsService,
    ToolsMenu toolsMenu) : IObsPlugin, IDisposable
{
    private readonly IPubSubService _pubSubService = pubSubService;
    private readonly List<IDisposable> _controls = [];
    private GCHandle _selfHandle;
    private bool _disposed;

    public bool Initialize()
    {
        RegisterOnObsFrontendEvent();

        var dockPtr = dockShim.CreateDock(localizationService.T("Toolbar.Title"));
        qtControlContext.SetQtControlContext(dockPtr);

        _controls.Add(qtFactory.Create<Dock>());

        toolsMenu.Register();

        heartbeatService.Start(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        LoadSettings();

        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected unsafe void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (_selfHandle.IsAllocated)
        {
            var userData = GCHandle.ToIntPtr(_selfHandle);

            delegate* unmanaged[Cdecl]<obs_frontend_event, void*, void> callback = &OnObsFrontendEvent;

            obsAdapter.RemoveEventCallback((IntPtr)callback, userData);

            _selfHandle.Free();
        }

        foreach (var control in _controls)
        {
            control.Dispose();
        }

        dockShim.DestroyDock();

        _disposed = true;
    }

    private unsafe void RegisterOnObsFrontendEvent()
    {
        _selfHandle = GCHandle.Alloc(this);
        var userData = GCHandle.ToIntPtr(_selfHandle);

        delegate* unmanaged[Cdecl]<obs_frontend_event, void*, void> callback = &OnObsFrontendEvent;

        obsAdapter.AddEventCallback((IntPtr)callback, userData);
    }

    private void LoadSettings()
    {
        var settings = settingsService.Load();

        _pubSubService.Raise(new SettingsChangedEvent(settings));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnObsFrontendEvent(obs_frontend_event eventType, void* privateData)
    {
        var handle = GCHandle.FromIntPtr((nint)privateData);
        var instance = (Plugin)handle.Target!;

        instance._pubSubService.Raise(new ObsFrontendEvent(eventType));
    }
}