using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.LogManager
{
  public class SearchViewModel : ViewModelBase
  {
    public Command HideSearchPanelCommand { get; private set; }
    public Command SearchCommand { get; private set; }
    public Command ClearCommand { get; private set; }
    public Command ViewAllCommand { get; private set; }
    public Command ViewSelectedCommand { get; private set; }
    public Command CopyAllCommand { get; private set; }
    public Command CopySelectedCommand { get; private set; }
    public string StringToSearch {
      get {
        return this.stringToSearch;
      }
      set { 
        this.stringToSearch = value;
        this.OnPropertyChanged();
      }
    }
    private string stringToSearch;
    public ObservableCollection<SearchEntryViewModel> SearchEntries { get; set; }
    public DateTime? LastSearchDateTime
    {
      get { return this.lastSearchDateTime; }
      protected set {
        this.lastSearchDateTime = value;
        this.OnPropertyChanged();
      }
    }
    private DateTime? lastSearchDateTime;
    public LogViewModel LogViewModel
    {
      get { return this.logViewModel; }
      protected set
      {
        this.logViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private LogViewModel logViewModel;

    public SearchViewModel()
    {
      this.SearchEntries = new ObservableCollection<SearchEntryViewModel>();
      this.InitCommands();
    }

    public SearchViewModel(LogViewModel logViewModel) : this()
    {
      this.LogViewModel = logViewModel;
    }

    public void Search()
    {
      this.SearchEntries.Clear();
      if (string.IsNullOrEmpty(this.StringToSearch))
        return;

      var searchRequest = new SearchRequest();
      searchRequest.Statements.Add(new TextStatement() { Text = this.StringToSearch });
      var searchResponse = SearchEngine.Search(this.LogViewModel.Log, searchRequest);
      foreach (var searchEntry in searchResponse.Entries)
        this.SearchEntries.Add(new SearchEntryViewModel(searchEntry));
      this.LastSearchDateTime = DateTime.Now;
    }

    public void ShowSerchEntryLine(SearchEntryViewModel searchEntryViewModel)
    {
      this.LogViewModel.ShowLogLine(searchEntryViewModel);
    }

    public void ViewSelected(IEnumerable<SearchEntryViewModel> searchEntries)
    {
      this.LogViewModel.SelectLogLines(searchEntries);
    }

    public void CopySearchEntriesToClipboard()
    {
      this.CopySearchEntriesToClipboard(this.SearchEntries.Select(x => x.SearchEntry));
    }

    public void CopySearchEntriesToClipboard(IEnumerable<SearchEntry> searchEntries)
    {
      var sb = new StringBuilder();
      foreach (var value in searchEntries.Select(x => x.Message?.RawValue).Where(x => x != null))
        sb.AppendLine(value);
      Clipboard.SetText(sb.ToString());
    }

    private void InitCommands()
    {
      this.HideSearchPanelCommand = new Command(
        x => {
          this.LogViewModel.State.ShowSearchPanel = false;
        },
        x => true
      );
      this.SearchCommand = new Command(
        x => {
          var stringToSearch = x as string;
          if (!string.IsNullOrWhiteSpace(stringToSearch))
            this.StringToSearch = stringToSearch;
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
      this.ViewAllCommand = new Command(
        x => { 
          this.ViewSelected(this.SearchEntries);
          this.LogViewModel.State.ShowSelectedMessagesPanel = true;
        },
        x => true
        );
      this.ViewSelectedCommand = new Command(
        x => {
          var collectionParameter = x as IEnumerable<object>;
          if (collectionParameter == null)
            return;
          var searchEntries = collectionParameter.Cast<SearchEntryViewModel>();
          if (searchEntries.Any())
            this.ViewSelected(searchEntries);
          this.LogViewModel.State.ShowSelectedMessagesPanel = true;
        },
        x => true
        );
      this.CopyAllCommand = new Command(
        x => {
          this.CopySearchEntriesToClipboard();
        },
        x => true
      );
      this.CopySelectedCommand = new Command(
        x => {
          var collectionParameter = x as IEnumerable<object>;
          if (collectionParameter == null)
            return;
          var searchEntries = collectionParameter.Cast<SearchEntryViewModel>();
          if (searchEntries.Any())
            this.CopySearchEntriesToClipboard(searchEntries.Select(x => x.SearchEntry));
        },
        x => true
      );
    }
  }
}
