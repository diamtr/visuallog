using System.Diagnostics;
using System.IO;

namespace VisualLog.Desktop.Dashboard
{
  public class RecentLogViewModel : ViewModelBase
  {
    public RecentLogInfo LogInfo { get; private set; }
    public Command OpenCommand { get; private set; }
    public Command RemoveCommand { get; private set; }
    public Command ShowInExplorerCommand { get; private set; }

    public RecentLogsViewModel Parent { get; set; }

    public RecentLogViewModel(RecentLogInfo logInfo, RecentLogsViewModel parent) : this()
    {
      this.LogInfo = logInfo;
      this.Parent = parent;
    }

    public RecentLogViewModel()
    {
      this.InitCommands();
    }

    private void InitCommands()
    {
      this.OpenCommand = new Command(
        x => this.Parent?.Open(this.LogInfo.Path),
        x => this.LogInfo != null);

      this.RemoveCommand = new Command(
        x => this.Parent?.Forget(this.LogInfo.Path),
        x => this.LogInfo != null);

      this.ShowInExplorerCommand = new Command(
        x =>
        {
          if (File.Exists(this.LogInfo.Path))
            Process.Start("explorer.exe", $"/select,\"{this.LogInfo.Path}\"");
          else
            this.Parent?.Forget(this.LogInfo.Path);
        },
        x => this.LogInfo != null);
    }
  }
}
