namespace VisualLog.Desktop.Dashboard
{
  public class OpenedLogsViewModel : ViewModelBase
  {
    private MainViewModel mainViewModel;
    public MainViewModel MainViewModel
    {
      get { return this.mainViewModel; }
      set
      {
        this.mainViewModel = value;
        this.OnPropertyChanged();
      }
    }
    
    public OpenedLogsViewModel(MainViewModel mainViewModel) : this()
    {
      this.mainViewModel = mainViewModel;
    }

    public OpenedLogsViewModel() { }
  }
}
