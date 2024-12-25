using System;

namespace VisualLog.Desktop.Dashboard
{
  public struct RecentLogInfo
  {
    public string DisplayName { get; set; }
    public string Path { get; set; }
    public DateTime LastOpened { get; set; }
  }
}
