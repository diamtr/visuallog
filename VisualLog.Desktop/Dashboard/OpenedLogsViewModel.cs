namespace VisualLog.Desktop.Dashboard
{
  public class OpenedLogsViewModel : ViewModelBase
  {
    public Command ShowOpenedLogsCommand { get; private set; }

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
      this.InitCommands();
    }

    public void InitCommands()
    {
      this.ShowOpenedLogsCommand = new Command(
        x => {
          if (this.dashboardViewModel != null &&
              this.dashboardViewModel.MainViewModel != null)
            this.dashboardViewModel.MainViewModel.SetAsActive(this.dashboardViewModel.MainViewModel.LogManagerViewModel);
        },
        x => true
        );
    }
  }
}
