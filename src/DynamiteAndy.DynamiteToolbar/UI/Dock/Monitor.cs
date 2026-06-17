using DynamiteAndy.DynamiteToolbar.Factories;
using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock;

public class Monitor : QFrame
{
    private readonly List<LabelGroup> _controls = [];

    public Monitor(
        IQtControlContext qtControlContext,
        IQtFactory qtFactory,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Monitor.MonitorPtr, qWidgetShim)
    {
        _controls.Add(qtFactory.CreateLabelGroup<StreamingLabel>());
        _controls.Add(qtFactory.CreateLabelGroup<RecordingLabel>());
        _controls.Add(qtFactory.CreateLabelGroup<CurrentSceneLabel>());
        _controls.Add(qtFactory.CreateLabelGroup<CpuLabel>());
        _controls.Add(qtFactory.CreateLabelGroup<FpsLabel>());
        _controls.Add(qtFactory.CreateLabelGroup<DroppedFramesLabel>());
        _controls.Add(qtFactory.CreateLabelGroup<KbpsLabel>());

        foreach (var c in _controls)
        {
            c.VisibilityChanged += OnVisibilityChanged;
        }
    }

    private void OnVisibilityChanged()
    {
        var visible = _controls.Where(c => c.Visible).ToList();

        for (var i = 0; i < visible.Count; i++)
        {
            var isLast = i == visible.Count - 1;
            visible[i].UpdateSeparator(isLast);
        }

        Visible = visible.Count > 0;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        foreach (var control in _controls)
        {
            control.VisibilityChanged -= OnVisibilityChanged;
            control.Dispose();
        }

        base.Dispose(disposing);
    }
}