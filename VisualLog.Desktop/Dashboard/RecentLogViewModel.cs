namespace VisualLog.Desktop.Dashboard
{
  public class RecentLogViewModel : ViewModelBase
  {
    public RecentLogInfo LogInfo { get; private set; }
    public Command OpenCommand { get; private set; }

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
        x => {
          this.Parent?.Open(this.LogInfo.Path); 
        },
        x => true
        );
    }
  }
}
