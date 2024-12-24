using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class SearchViewModel : ViewModelBase
  {
    public Command HideSearchPanelCommand { get; private set; }
    public Command SearchCommand { get; private set; }
    public Command ClearCommand { get; private set; }
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

      var searchResults = this.LogViewModel.Log.SearchString(this.StringToSearch);
      foreach (var searchEntry in searchResults.Entries)
        this.SearchEntries.Add(new SearchEntryViewModel(searchEntry));
      this.LastSearchDateTime = DateTime.Now;
    }

    public void ShowSerchEntryLine(SearchEntryViewModel searchEntryViewModel)
    {
      this.LogViewModel.ShowLogLine(searchEntryViewModel);
    }

    public void CopySearchEntriesToClipboard()
    {
      this.CopySearchEntriesToClipboard(this.SearchEntries.Select(x => x.SearchEntry));
    }

    public void CopySearchEntriesToClipboard(IEnumerable<SearchEntry> searchEntries)
    {
      var sb = new StringBuilder();
      foreach (var value in searchEntries.Select(x => x.RawString))
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
