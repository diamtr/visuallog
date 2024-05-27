using System.Collections.Generic;

namespace VisualLog.Capture.Search
{
  public interface IStoreResultsStrategy
  {
    public void Store(PathBuilder pathbuilder, IEnumerable<SearchResult> searchResults);
  }
}
