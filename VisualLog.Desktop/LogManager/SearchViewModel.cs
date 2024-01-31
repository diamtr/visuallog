using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace VisualLog.Desktop.LogManager
{
  public class SearchViewModel : ViewModelBase
  {
    public Command HideSearchPanelCommand { get; private set; }
    public Command SearchCommand { get; private set; }
    public Command ClearCommand { get; private set; }
    private string stringToSearch;
    public string StringToSearch {
      get {
        return this.stringToSearch;
      }
      set { 
        this.stringToSearch = value;
        this.OnPropertyChanged();
      }
    }
    public ObservableCollection<SearchEntryViewModel> SearchEntries { get; set; }

    private LogViewModel logViewModel;

    public SearchViewModel()
    {
      this.SearchEntries = new ObservableCollection<SearchEntryViewModel>();
      this.InitCommands();
    }

    public SearchViewModel(LogViewModel logViewModel) : this()
    {
      this.logViewModel = logViewModel;
    }

    public void Search()
    {
      this.SearchEntries.Clear();
      if (string.IsNullOrEmpty(this.StringToSearch))
        return;

      var searchResults = this.logViewModel.Log.SearchString(this.StringToSearch);
      foreach (var searchEntry in searchResults.Entries)
        this.SearchEntries.Add(new SearchEntryViewModel(searchEntry));
    }

    public void ShowSerchEntryLine(SearchEntryViewModel searchEntryViewModel)
    {
      this.logViewModel.ShowLogLine(searchEntryViewModel);
    }

    private void InitCommands()
    {
      this.HideSearchPanelCommand = new Command(
        x => {
          this.logViewModel.State.ShowSearchPanel = false;
        },
        x => true
      );
      this.SearchCommand = new Command(
        x => {
          this.Search();
        },
        x => true
      );
      this.ClearCommand = new Command(
        x => {
          this.StringToSearch = string.Empty;
          this.SearchEntries.Clear();
        },
        x => true
      );
    }
  }
}
