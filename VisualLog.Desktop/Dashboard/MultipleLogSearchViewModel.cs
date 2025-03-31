using System.Collections.ObjectModel;
using System.Linq;
using VisualLog.Core.Search;
using VisualLog.Desktop.Search;

namespace VisualLog.Desktop.Dashboard
{
  public class MultipleLogSearchViewModel : ViewModelBase
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

    public SearchRequestViewModel SearchRequestViewModel
    {
      get { return this.searchRequestViewModel; }
      set
      {
        this.searchRequestViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private SearchRequestViewModel searchRequestViewModel;

    public ObservableCollection<SearchResultsViewModel> SearchResults { get; set; }

    public MultipleLogSearchViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.dashboardViewModel = dashboardViewModel;
    }

    public MultipleLogSearchViewModel()
    {
      this.SearchRequestViewModel = new SearchRequestViewModel();
      this.SearchRequestViewModel.SearchRequested += SearchRequestViewModel_SearchRequested;
      this.SearchResults = new ObservableCollection<SearchResultsViewModel>();
    }

    private void SearchRequestViewModel_SearchRequested(SearchRequest searchRequest)
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
        var searchResults = SearchEngine.Search(logViewModel.Log, searchRequest);
        if (searchResults.Entries.Any())
          this.SearchResults.Add(new SearchResultsViewModel(searchResults));
      }
    }
  }
}
