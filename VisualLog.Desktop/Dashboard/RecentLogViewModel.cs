using Newtonsoft.Json;
using System;

namespace VisualLog.Desktop.Dashboard
{
  public class RecentLogViewModel : ViewModelBase
  {
    public string DisplayName { get; set; }
    public string Path { get; set; }
    public DateTime LastOpened { get; set; }

    [JsonIgnore]
    public Command OpenCommand { get; private set; }

    [JsonIgnore]
    public RecentLogsViewModel Parent { get; set; }

    public RecentLogViewModel()
    {
      this.InitCommands();
    }

    public override bool Equals(object obj)
    {
      var recentLog = obj as RecentLogViewModel;
      if (recentLog == null)
        return false;
      return
        this.DisplayName == recentLog.DisplayName &&
        this.Path == recentLog.Path &&
        this.LastOpened == recentLog.LastOpened;
    }

    public override int GetHashCode()
    {
      return
        this.DisplayName.GetHashCode() ^
        this.Path.GetHashCode() ^
        this.LastOpened.GetHashCode();
    }

    private void InitCommands()
    {
      this.OpenCommand = new Command(
        x => { this.Parent?.Open(this.Path); },
        x => true
        );
    }
  }
}
