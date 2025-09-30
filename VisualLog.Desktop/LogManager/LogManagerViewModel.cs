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
        this.activeLog = value;
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

    public LogManagerViewModel(MainViewModel mainViewModel) : this()
    {
      this.MainViewModel = mainViewModel;
    }
    public LogManagerViewModel() { }
  }
}
