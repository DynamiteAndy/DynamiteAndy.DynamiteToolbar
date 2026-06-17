using DynamiteAndy.DynamiteToolbar.Factories;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.UI.Dialog;
using DynamiteAndy.DynamiteToolbar.UI.Dock;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Spacers;
using DynamiteAndy.DynamiteToolbar.UI.Menu;
using Microsoft.Extensions.DependencyInjection;
using DockMonitor = DynamiteAndy.DynamiteToolbar.UI.Dock.Monitor;

namespace DynamiteAndy.DynamiteToolbar.DependencyInjection;

public static class UIServiceExtensions
{
    public static void AddUIServices(this IServiceCollection services)
    {
        services.AddSingleton<IQtControlContext, QtControlContext>();
        services.AddSingleton<IQtFactory, QtFactory>();
        services.AddSingleton<SettingsDialog>();
        services.AddSingleton<ToolsMenu>();
        services.AddTransient<Dock>();
        services.AddTransient<DockMonitor>();

        AddButtons(services);
        AddLabels(services);
    }

    private static void AddButtons(IServiceCollection services)
    {
        services.AddTransient<FloatButton>();
        services.AddTransient<FloatSpacer>();
        services.AddTransient<StudioModeButton>();
        services.AddTransient<VirtualCameraButton>();
        services.AddTransient<VirtualCameraSpacer>();
        services.AddTransient<ReplayBufferButton>();
        services.AddTransient<ReplayBufferSpacer>();
        services.AddTransient<RecordButton>();
        services.AddTransient<RecordPausedButton>();
        services.AddTransient<RecordSpacer>();
        services.AddTransient<StreamButton>();
    }

    private static void AddLabels(IServiceCollection services)
    {
        services.AddTransient<StreamingLabel>();
        services.AddTransient<RecordingLabel>();
        services.AddTransient<CurrentSceneLabel>();
        services.AddTransient<CpuLabel>();
        services.AddTransient<FpsLabel>();
        services.AddTransient<DroppedFramesLabel>();
        services.AddTransient<KbpsLabel>();
    }
}