using Avalonia.Platform;
using System.Globalization;
using System.Text.Json;

namespace SPRView.Net.ViewModel;

/// <summary>
/// Loads <see cref="LangViewModel"/> from the embedded language assets.
/// Shared by every window so a new dialog gets localized text for free.
/// </summary>
public static class LangLoader
{
    /// <summary>Loads the language matching the OS, falling back to English.</summary>
    public static LangViewModel Load() =>
        Load(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

    public static LangViewModel Load(string lang)
    {
        Stream asset;
        try
        {
            asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/{lang}.json"));
        }
        catch (Exception)
        {
            asset = AssetLoader.Open(new Uri("avares://SPRView.Net/Assets/Lang/en.json"));
        }

        using var reader = new StreamReader(asset);
        string json = reader.ReadToEnd();
        return JsonSerializer.Deserialize(json, LangJsonContext.Default.LangViewModel)
            ?? new LangViewModel();
    }
}
