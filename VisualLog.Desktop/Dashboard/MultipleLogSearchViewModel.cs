namespace VisualLog.Desktop.Dashboard
{
  public class MultipleLogSearchViewModel : ViewModelBase
  {
    public Command SearchInOpenedLogsCommand { get; private set; }

    public string StringToSearch
    {
      get
      {
        return this.stringToSearch;
      }
      set
      {
        this.stringToSearch = value;
        this.OnPropertyChanged();
      }
    }
    private string stringToSearch;

    private DashboardViewModel dashboardViewModel;

    public MultipleLogSearchViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.dashboardViewModel = dashboardViewModel;
    }

    public MultipleLogSearchViewModel()
    {
      this.InitCommands();
    }

    public void InitCommands()
    {
      this.SearchInOpenedLogsCommand = new Command(
        x => { },
        x => true
        );
    }
  }
}
