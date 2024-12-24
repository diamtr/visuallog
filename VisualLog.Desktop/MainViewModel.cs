using VisualLog.Desktop.Dashboard;
using VisualLog.Desktop.FormatManager;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop
{
  public class MainViewModel : ViewModelBase
  {
    public ViewModelBase ActiveViewModel
    {
      get { return this.activeViewModel; }
      private set { this.activeViewModel = value; this.OnPropertyChanged(); }
    }
    private ViewModelBase activeViewModel;

    public DashboardViewModel DashboardViewModel { get; private set; }
    public LogManagerViewModel LogManagerViewModel { get; private set; }
    public FormatManagerViewModel FormatManagerViewModel { get; private set; }
    public Command ShowDashboardCommand { get; private set; }
    public Command ShowLogManagerCommand { get; private set; }
    public Command ShowFormatManagerCommand { get; private set; }

    public MainViewModel()
    {
      this.DashboardViewModel = new DashboardViewModel(this);
      this.LogManagerViewModel = new LogManagerViewModel(this);
      this.LogManagerViewModel.Logs.CollectionChanged += LogManagerViewModelLogsCollectionChanged;
      this.FormatManagerViewModel = new FormatManagerViewModel();
      this.InitCommands();
    }

    public void OnWindowLoaded()
    {
      this.SetAsActive(this.DashboardViewModel);
    }

    public void OnWindowClosing()
    {
      if (this.DashboardViewModel != null)
        this.DashboardViewModel.RememberOpenedLogs();
    }

    public void SetAsActive(ViewModelBase viewModel)
    {
      this.ActiveViewModel = viewModel;
    }

    private void InitCommands()
    {
      this.ShowDashboardCommand = new Command(
        x => { this.SetAsActive(this.DashboardViewModel); },
        x => true
        );
      this.ShowLogManagerCommand = new Command(
        x => { this.SetAsActive(this.LogManagerViewModel); },
        x => true
        );
      this.ShowFormatManagerCommand = new Command(
        x => { this.SetAsActive(this.FormatManagerViewModel); },
        x => true
        );
    }

    private void LogManagerViewModelLogsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      this.SetAsActive(this.LogManagerViewModel);
    }
  }
}
