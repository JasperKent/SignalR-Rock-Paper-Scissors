using System.Windows.Input;

namespace WpfClient;

internal class DelegateCommand : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    private readonly Action<object?> _execute;
    private readonly Predicate<object?> _canExecute;

    public DelegateCommand(Action<object?> action, Predicate<object?>? canExecute = null)
    {
        _execute = action;
        _canExecute = canExecute ?? (_ => true);
    }

    public bool CanExecute(object? parameter) => _canExecute(parameter);

    public void Execute(object? parameter) => _execute(parameter);
}
