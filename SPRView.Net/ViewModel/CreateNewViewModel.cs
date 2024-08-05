using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
namespace SPRView.Net.ViewModel;

public class CreateNewViewModel(Window parent) : INotifyPropertyChanged
{
    public Window Parent = parent;
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

    public int PlaySpeed { get; set; } = 24;
    public int Type { get; set; } = 0;
    public int Format { get; set; } = 0;
    public int Sync {  get; set; } = 0;

    public async void AddImage()
    {
        var files = await Parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang?.CreateNew_AddImage_Title,
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll],
        });
        if (files.Count >= 1)
        {
            await using Stream file = await files[0].OpenReadAsync();
            App.LoadFile(file);
        }
    }
    public void RemoveImage()
    {

    }
    public void MoveupImage()
    {

    }
    public void MovedownImage()
    {

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
}
