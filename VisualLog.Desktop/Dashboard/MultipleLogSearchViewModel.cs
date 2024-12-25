using System.Collections.ObjectModel;
using System.Linq;

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

    public ObservableCollection<SearchResultsViewModel> SearchResults { get; set; }

    public MultipleLogSearchViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.dashboardViewModel = dashboardViewModel;
    }

    public MultipleLogSearchViewModel()
    {
      this.SearchResults = new ObservableCollection<SearchResultsViewModel>();
      this.InitCommands();
    }

    public void InitCommands()
    {
      this.SearchInOpenedLogsCommand = new Command(
        x => { this.SearchInOpenedLogs(); },
        x => true
        );
    }

    public void SearchInOpenedLogs()
    {
      if (this.DashboardViewModel == null ||
          this.DashboardViewModel.MainViewModel == null ||
          this.DashboardViewModel.MainViewModel.LogManagerViewModel == null ||
          !this.DashboardViewModel.MainViewModel.LogManagerViewModel.Logs.Any())
        return;

      this.SearchResults.Clear();
      var logViewModels = this.DashboardViewModel.MainViewModel.LogManagerViewModel.Logs;
      foreach (var logViewModel in logViewModels)
      {
        var searchResults = logViewModel.Log.SearchString(this.StringToSearch);
        if (searchResults.Entries.Any())
          this.SearchResults.Add(new SearchResultsViewModel(searchResults));
      }
    }
  }
}
