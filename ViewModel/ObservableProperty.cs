using System.Collections.Generic;
using System.ComponentModel;

namespace SPRView.Net.ViewModel;

public class ObservableProperty : INotifyPropertyChanged
{
    private string _value;
    private readonly string _propertyName;

    public ObservableProperty(string propertyName, string initialValue = "[NULLSTR]")
    {
        _propertyName = propertyName;
        _value = initialValue;
    }

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(_propertyName);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return _value;
    }
}