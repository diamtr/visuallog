namespace VisualLog.Desktop.Dashboard
{
  public class DashboardViewModel : ViewModelBase
  {
    public MainViewModel MainViewModel
    {
      get { return this.mainViewModel; }
      private set
      {
        this.mainViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private MainViewModel mainViewModel;
    public MultipleLogSearchViewModel MultipleLogSearchViewModel
    {
      get { return this.multipleLogSearchViewModel; }
      protected set
      {
        this.multipleLogSearchViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private MultipleLogSearchViewModel multipleLogSearchViewModel;
    public OpenedLogsViewModel OpenedLogsViewModel
    {
      get { return this.openedLogsViewModel; }
      protected set
      {
        this.openedLogsViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private OpenedLogsViewModel openedLogsViewModel;
    public RecentLogsViewModel RecentLogsViewModel
    {
      get { return this.recentLogsViewModel; }
      protected set
      {
        this.recentLogsViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private RecentLogsViewModel recentLogsViewModel;

    public DashboardViewModel(MainViewModel mainViewModel) : this()
    {
      this.MainViewModel = mainViewModel;
    }
    public DashboardViewModel()
    {
      this.MultipleLogSearchViewModel = new MultipleLogSearchViewModel(this);
      this.OpenedLogsViewModel = new OpenedLogsViewModel(this);
      this.RecentLogsViewModel = new RecentLogsViewModel(this);
    }

    public void RememberOpenedLogs()
    {
      this.RecentLogsViewModel.RememberOpenedLogs();
    }
  }
}
