using DynamiteAndy.DynamiteToolbar.Interop.Obs;

namespace DynamiteAndy.DynamiteToolbar.Services;

public sealed class ObsLocalizationService(IObsApi obsApi, IObsTextLookupApi obsTextLookupApi) : ILocalizationService, IDisposable
{
    private IntPtr _currentLookup = IntPtr.Zero;
    public string DefaultLocale { get; private set; } = "en-GB";

    public void SetLocale(string locale)
    {
        if (_currentLookup != IntPtr.Zero)
        {
            obsTextLookupApi.Destroy(_currentLookup);
        }

        _currentLookup = obsApi.ModuleLoadLocale(DefaultLocale, locale);
    }

    public string T(string identifier)
        => _currentLookup == IntPtr.Zero
            ? identifier
            : obsTextLookupApi.GetString(_currentLookup, identifier) ?? identifier;


    public void Dispose()
    {
        if (_currentLookup == IntPtr.Zero)
        {
            return;
        }

        obsTextLookupApi.Destroy(_currentLookup);
        _currentLookup = IntPtr.Zero;
    }
}