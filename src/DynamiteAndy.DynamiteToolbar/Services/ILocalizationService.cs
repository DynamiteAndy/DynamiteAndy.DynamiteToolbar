namespace DynamiteAndy.DynamiteToolbar.Services;

public interface ILocalizationService
{
    string DefaultLocale { get; }
    void SetLocale(string locale);
    string T(string identifier);
    void Dispose();
}