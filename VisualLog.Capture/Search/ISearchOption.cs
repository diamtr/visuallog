namespace VisualLog.Capture.Search
{
  public interface ISearchOption
  {
    public string FilePrefix { get; set; }
    public SearchResult Search(string filePath);
  }
}
