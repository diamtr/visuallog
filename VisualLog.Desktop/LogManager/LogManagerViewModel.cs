using System.Linq;

namespace VisualLog.Desktop.LogManager
{
  public class LogManagerViewModel : ViewModelBase
  {
    public LogViewModel ActiveLog
    {
      get
      {
        return this.activeLog;
      }
      set
      {
        if (ReferenceEquals(this.activeLog, value))
          return;
        if (this.activeLog != null)
          this.activeLog.IsActive = false;
        this.activeLog = value;
        if (this.activeLog != null)
          this.activeLog.IsActive = true;
        this.OnPropertyChanged();
      }
    }
    private LogViewModel activeLog;

    public MainViewModel MainViewModel
    {
      get { return this.mainViewModel; }
      protected set
      {
        this.mainViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private MainViewModel mainViewModel;

    /// <summary>Activates the next opened log (wraps around), Ctrl+Tab.</summary>
    public Command NextLogCommand { get; private set; }
    /// <summary>Activates the previous opened log (wraps around), Ctrl+Shift+Tab.</summary>
    public Command PreviousLogCommand { get; private set; }
    /// <summary>Closes the active log, Ctrl+W.</summary>
    public Command CloseActiveLogCommand { get; private set; }

    public LogManagerViewModel(MainViewModel mainViewModel) : this()
    {
      this.MainViewModel = mainViewModel;
    }

    public LogManagerViewModel()
    {
      this.NextLogCommand = new Command(x => this.ActivateRelative(1), x => this.CanSwitch());
      this.PreviousLogCommand = new Command(x => this.ActivateRelative(-1), x => this.CanSwitch());
      this.CloseActiveLogCommand = new Command(
        x => this.ActiveLog?.CloseCommand.Execute(null),
        x => this.ActiveLog != null);
    }

    private bool CanSwitch()
    {
      return this.MainViewModel != null && this.MainViewModel.Logs.Count > 1;
    }

    private void ActivateRelative(int offset)
    {
      var logs = this.MainViewModel?.Logs;
      if (logs == null || !logs.Any())
        return;

      var index = this.ActiveLog != null ? logs.IndexOf(this.ActiveLog) : -1;
      if (index < 0)
      {
        this.ActiveLog = logs.First();
        return;
      }
      var count = logs.Count;
      this.ActiveLog = logs[((index + offset) % count + count) % count];
    }
  }
}
