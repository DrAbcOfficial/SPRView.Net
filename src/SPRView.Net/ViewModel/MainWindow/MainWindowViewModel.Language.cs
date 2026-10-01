using System.Globalization;

namespace SPRView.Net.ViewModel;

/// <summary>
/// User interface language loading and switching.
/// </summary>
public partial class MainWindowViewModel
{
    private LangViewModel? _lang;
    public LangViewModel? Lang
    {
        get => _lang;
        set
        {
            if (ReferenceEquals(_lang, value))
                return;
            _lang = value;
            OnPropertyChanged(nameof(Lang));
            OnPropertyChanged(nameof(PaletteCountLabel));
        }
    }

    public void ChangeLang(string lang) => LoadLangFile(lang);

    public void LoadLangFile() =>
        LoadLangFile(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

    public void LoadLangFile(string lang) => Lang = LangLoader.Load(lang);
}
