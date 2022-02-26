using System;
using System.Windows.Input;

namespace VisualLog.Desktop
{
  public class Command : ICommand
  {
    private Action<object> execute;
    private Func<object, bool> canExecute;

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return this.canExecute == null || this.canExecute(parameter);
    }

    public void Execute(object parameter)
    {
      this.execute(parameter);
    }

    public Command(Action<object> execute, Func<object, bool> canExecute = null)
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }
  }
}
