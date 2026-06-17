using DynamiteAndy.DynamiteToolbar.Factories;
using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Spacers;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock;

public class Dock : QDockWidget
{
    private readonly IPubSubService _pubSubService;
    private readonly List<IDisposable> _controls = [];

    public Dock(
        IQtControlContext qtControlContext,
        IQtFactory qtFactory,
        IPubSubService pubSubService,
        IQDockWidgetShim qDockWidgetShim) : base(qtControlContext.Dock!.Value.DockPtr, qDockWidgetShim)
    {
        _pubSubService = pubSubService;

        _controls.Add(qtFactory.Create<Monitor>());
        _controls.Add(qtFactory.CreateButton<FloatButton>());
        _controls.Add(qtFactory.Create<FloatSpacer>());
        _controls.Add(qtFactory.CreateButton<StudioModeButton>());
        _controls.Add(qtFactory.CreateButton<VirtualCameraButton>());
        _controls.Add(qtFactory.Create<VirtualCameraSpacer>());
        _controls.Add(qtFactory.CreateButton<ReplayBufferButton>());
        _controls.Add(qtFactory.Create<ReplayBufferSpacer>());
        _controls.Add(qtFactory.CreateButton<RecordButton>());
        _controls.Add(qtFactory.CreateButton<RecordPausedButton>());
        _controls.Add(qtFactory.Create<RecordSpacer>());
        _controls.Add(qtFactory.CreateButton<StreamButton>());
    }

    protected override void OnTopLevelChanged(bool topLevel) => _pubSubService.Raise(new TopLevelChangedEvent(topLevel));

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        foreach (var control in _controls)
        {
            control.Dispose();
        }

        base.Dispose(disposing);
    }
}