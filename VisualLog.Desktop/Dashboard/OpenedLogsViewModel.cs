namespace VisualLog.Desktop.Dashboard
{
  public class OpenedLogsViewModel : ViewModelBase
  {
    public DashboardViewModel DashboardViewModel
    {
      get { return this.dashboardViewModel; }
      protected set
      {
        this.dashboardViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private DashboardViewModel dashboardViewModel;

    public OpenedLogsViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.DashboardViewModel = dashboardViewModel;
    }
    public OpenedLogsViewModel()
    {
    }
  }
}
