﻿using System.Collections.ObjectModel;
using VisualLog.Core.Search;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Dashboard
{
  public class SearchResultsViewModel : ViewModelBase
  {
    public ObservableCollection<SearchEntryViewModel> SearchEntries { get; set; }
    public SearchResponse SearchResults
    {
      get { return this.searchResults; }
      protected set
      {
        this.searchResults = value;
        this.OnPropertyChanged();
      }
    }
    private SearchResponse searchResults;

    public SearchResultsViewModel(SearchResponse searchResults) : this()
    {
      this.SearchResults = searchResults;
      this.UpdateSearchEntries();
    }
    public SearchResultsViewModel()
    {
      this.SearchEntries = new ObservableCollection<SearchEntryViewModel>();
    }

    public void UpdateSearchEntries()
    {
      this.SearchEntries.Clear();
      foreach (var entry in this.SearchResults.Entries)
        this.SearchEntries.Add(new SearchEntryViewModel(entry));
    }
  }
}
