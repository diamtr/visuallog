namespace VisualLog.Core
{
  public class MessageFormatter
  {
    public string Name { get; set; }
    public int Priority { get; set; }
    public string Pattern { get; set; }
    public bool Required { get; set; }
    public string Extract(string input)
    {
      return input;
    }
  }
}
