using Avalonia.Controls;
using Avalonia.Platform;
using SPRView.Net.Core;
using SPRView.Net.Storage;
using System.Globalization;
using System.Text.Json;

namespace SPRView.Net.ViewModel;

/// <summary>
/// User interface language loading and switching.
/// </summary>
public partial class MainWindowViewModel
{
    private LangViewModel? _lang = null;
    public LangViewModel? Lang
    {
        get => _lang;
        set
        {
            if (_lang != value)
                _lang = value;
        }
    }

    public void ChangeLang(string lang) => LoadLangFile(lang);

    public void LoadLangFile() =>
        LoadLangFile(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

    public void LoadLangFile(string lang)
    {
        Stream asset;
        try
        {
            asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/{lang}.json"));
        }
        catch (Exception)
        {
            asset = AssetLoader.Open(new Uri($"avares://SPRView.Net/Assets/Lang/en.json"));
        }
        LoadLangFileInternal(asset);
    }

    private void LoadLangFileInternal(Stream file)
    {
        using var reader = new StreamReader(file);
        string json = reader.ReadToEnd();
        LangViewModel? person = JsonSerializer.Deserialize(json, LangJsonContext.Default.LangViewModel);
        if (person != null)
            Lang = person;
        file.Dispose();
        OnPropertyChanged(nameof(Lang));
    }
}
