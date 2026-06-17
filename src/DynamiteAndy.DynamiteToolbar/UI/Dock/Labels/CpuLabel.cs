using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;

namespace DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;

public class CpuLabel : LabelGroup
{
    private readonly IObsPlatformApi _obsPlatformApi;
    private readonly ILocalizationService _localizationService;
    private readonly IPubSubService _pubSubService;

    private static IntPtr _cpuInfo;

    public CpuLabel(
        IQtControlContext qtControlContext,
        IObsPlatformApi obsPlatformApi,
        ILocalizationService localizationService,
        IPubSubService pubSubService,
        IQLabelShim qLabelShim,
        IQWidgetShim qWidgetShim) : base(qtControlContext.Dock!.Value.Monitor.Cpu, qLabelShim, qWidgetShim)
    {
        _obsPlatformApi = obsPlatformApi;
        _localizationService = localizationService;
        _pubSubService = pubSubService;

        Name.Text = _localizationService.T("Toolbar.Label.CPU");

        if (_cpuInfo == IntPtr.Zero)
        {
            _cpuInfo = _obsPlatformApi.OSCpuUsageInfoStart();
        }

        _pubSubService.Subscribe<HeartbeatEvent>(OnHeartbeatEvent);
        _pubSubService.Subscribe<SettingsChangedEvent>(OnSettingsChangedEvent);
    }

    public void OnHeartbeatEvent(HeartbeatEvent e)
    {
        if (!Visible)
        {
            return;
        }

        Value.Text = _cpuInfo != IntPtr.Zero ? $"{_obsPlatformApi.OSCpuUsageInfoQuery(_cpuInfo):F1}%" : "0.0%";
    }

    public void OnSettingsChangedEvent(SettingsChangedEvent e) => Visible = !e.Settings.HideCpuLabel;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (_cpuInfo != IntPtr.Zero)
        {
            _obsPlatformApi.OSCpuUsageInfoDestroy(_cpuInfo);
            _cpuInfo = IntPtr.Zero;
        }

        _pubSubService.Unsubscribe<HeartbeatEvent>(OnHeartbeatEvent);
        _pubSubService.Unsubscribe<SettingsChangedEvent>(OnSettingsChangedEvent);

        base.Dispose(disposing);
    }
}