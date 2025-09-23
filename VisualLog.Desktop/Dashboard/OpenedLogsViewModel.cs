namespace VisualLog.Desktop.Dashboard
{
  public class OpenedLogsViewModel : ViewModelBase
  {
    private DashboardViewModel dashboardViewModel;
    public DashboardViewModel DashboardViewModel
    {
      get { return this.dashboardViewModel; }
      protected set
      {
        this.dashboardViewModel = value;
        this.OnPropertyChanged();
      }
    }

    public OpenedLogsViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.DashboardViewModel = dashboardViewModel;
    }
    public OpenedLogsViewModel()
    {
    }
  }
}
