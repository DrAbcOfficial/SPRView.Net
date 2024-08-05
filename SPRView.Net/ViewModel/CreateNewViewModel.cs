using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace SPRView.Net.ViewModel;

public class CreateNewViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private List<string> m_aryImagePaths = [];
    public string[] ImagePaths
    {
        get { return m_aryImagePaths.ToArray(); }
    }

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
    private CommandViewModel? _cmd = null;
    public CommandViewModel? Command
    {
        get => _cmd;
        set
        {
            if (_cmd != value)
                _cmd = value;
        }
    }
}
