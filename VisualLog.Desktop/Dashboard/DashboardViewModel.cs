using VisualLog.Desktop.Search;

namespace VisualLog.Desktop.Dashboard
{
  public class DashboardViewModel : ViewModelBase
  {
    private MainViewModel mainViewModel;
    public MainViewModel MainViewModel
    {
      get { return this.mainViewModel; }
      private set
      {
        this.mainViewModel = value;
        this.OnPropertyChanged();
      }
    }

    private OpenedLogsViewModel openedLogsViewModel;
    public OpenedLogsViewModel OpenedLogsViewModel
    {
      get { return this.openedLogsViewModel; }
      protected set
      {
        this.openedLogsViewModel = value;
        this.OnPropertyChanged();
      }
    }

    private RecentLogsViewModel recentLogsViewModel;
    public RecentLogsViewModel RecentLogsViewModel
    {
      get { return this.recentLogsViewModel; }
      protected set
      {
        this.recentLogsViewModel = value;
        this.OnPropertyChanged();
      }
    }

    private SearchViewModel searchViewModel;
    public SearchViewModel SearchViewModel
    {
      get { return this.searchViewModel; }
      protected set
      {
        this.searchViewModel = value;
        this.OnPropertyChanged();
      }
    }

    public DashboardViewModel(MainViewModel mainViewModel) : this()
    {
      this.MainViewModel = mainViewModel;
      this.OpenedLogsViewModel = new OpenedLogsViewModel(this.MainViewModel);
      this.SearchViewModel = new SearchViewModel(this.MainViewModel);
    }
    public DashboardViewModel()
    {
      this.RecentLogsViewModel = new RecentLogsViewModel(this);
    }

    public void RememberOpenedLogs()
    {
      this.RecentLogsViewModel.RememberOpenedLogs();
    }

    public void RememberLog(LogManager.LogViewModel logViewModel)
    {
      this.RecentLogsViewModel.RememberLog(logViewModel);
    }
  }
}
