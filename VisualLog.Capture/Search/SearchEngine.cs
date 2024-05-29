using NLog;
using System.Collections.Generic;

namespace VisualLog.Capture.Search
{
  public class SearchEngine
  {
    internal static ILogger log = LogManager.GetCurrentClassLogger();

    public PathBuilder PathBuilder { get; set; }
    public List<ISearchOption> SearchOptions { get; set; }
    public IStoreResultsStrategy StoreResultsStrategy { get; set; }
    public List<SearchResult> SearchResults { get; private set; }

    public SearchEngine()
    {
      this.SearchResults = new List<SearchResult>();
      this.SearchOptions = new List<ISearchOption>();
    }

    public void WithOption(ISearchOption option)
    {
      if (option == null)
        return;
      this.SearchOptions.Add(option);
    }

    public void Search()
    {
      this.SearchResults.Clear();
      log.Info("[ Search options ]");
      foreach (var searchOption in this.SearchOptions)
        log.Info($"{searchOption}");
      log.Info("[ Search in ]");
      foreach (var filePath in this.PathBuilder.Files)
      {
        log.Info($"{filePath}");
        this.SearchResults.AddRange(this.FileSearch(filePath));
      }
    }

    public List<SearchResult> FileSearch(string filePath)
    {
      var searchResults = new List<SearchResult>();
      foreach (var searchOption in this.SearchOptions)
        searchResults.Add(searchOption.Search(filePath));
      return searchResults;
    }

    public void StoreSearchResults()
    {
      this.StoreResultsStrategy.Store(this.PathBuilder, this.SearchResults);
    }
  }
}
