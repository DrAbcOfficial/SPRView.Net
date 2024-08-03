using System;
using System.Windows.Input;

namespace SPRView.Net.ViewModel;

public class CCommand(Action<object?> execute) : ICommand
{
    private readonly Action<object?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add {  }
        remove {  }
    }
}