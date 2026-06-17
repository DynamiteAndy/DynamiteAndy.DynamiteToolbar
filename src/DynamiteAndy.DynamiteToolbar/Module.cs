using DynamiteAndy.DynamiteToolbar.Interop.Native;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Services;
using Microsoft.Extensions.DependencyInjection;
using ObsInterop;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar;

public static class Module
{
    private static ILogger Logger => ModuleHost.Services.GetRequiredService<ILogger>();
    private static ILocalizationService LocalizationService => ModuleHost.Services.GetRequiredService<ILocalizationService>();
    private static IObsPlugin Plugin => ModuleHost.Services.GetRequiredService<IObsPlugin>();

    [UnmanagedCallersOnly(EntryPoint = "obs_module_set_pointer", CallConvs = [typeof(CallConvCdecl)])]
    public static void ObsModuleSetPointer(IntPtr obsModulePointer)
    {
        ModuleHost.Initialize();

        SetModuleContext(obsModulePointer);
        LoadModuleShim();
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_ver", CallConvs = [typeof(CallConvCdecl)])]
    public static uint ObsModuleVer()
    {
        var major = (uint)Obs.Version.Major;
        var minor = (uint)Obs.Version.Minor;
        var patch = (uint)Obs.Version.Build;
        return (major << 24) | (minor << 16) | patch;
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_load", CallConvs = [typeof(CallConvCdecl)])]
    public static bool ObsModuleLoad()
    {
        try
        {
            return Plugin.Initialize();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load: {ex.Message}");
            return false;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_set_locale", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void ObsModuleSetLocale(IntPtr locale)
    {
        var localeString = Marshal.PtrToStringUTF8(locale) ?? LocalizationService.DefaultLocale;
        LocalizationService.SetLocale(localeString);
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_free_locale", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void ObsModuleFreeLocale() => LocalizationService.Dispose();

    [UnmanagedCallersOnly(EntryPoint = "obs_module_unload", CallConvs = [typeof(CallConvCdecl)])]
    public static void ObsModuleUnload() => ModuleHost.Dispose();

    private static void SetModuleContext(IntPtr obsModulePointer)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name!;
        var context = ModuleHost.Services.GetRequiredService<IObsModuleContext>();

        context.SetModuleContext(name, obsModulePointer);
    }

    private static void LoadModuleShim()
    {
        var loader = ModuleHost.Services.GetRequiredService<INativeLibraryLoader>();

        loader.Load();
    }
}