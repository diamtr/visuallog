using System;

namespace VisualLog.Desktop.Dashboard
{
  public class RecentLogInfo
  {
    public string DisplayName { get; set; }
    public string PathDirectory
    {
      get
      {
        var pos = this.Path?.LastIndexOfAny(new char[] { '/', '\\' }) ?? -1;
        if (pos == -1)
          return this.Path;
        return this.Path.Substring(0, pos);
      }
    }
    public string Path { get; set; }
    public DateTime LastOpened { get; set; }

    public override bool Equals(object obj)
    {
      var recentLog = obj as RecentLogInfo;
      if (recentLog == null)
        return false;
      return
        this.DisplayName == recentLog.DisplayName &&
        this.Path == recentLog.Path;
    }

    public override int GetHashCode()
    {
      return
        this.DisplayName.GetHashCode() ^
        this.Path.GetHashCode();
    }
  }
}
