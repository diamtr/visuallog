using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace VisualLog.Desktop.Dashboard
{
  public class DashboardViewModel : ViewModelBase
  {
    public const string RecentFileName = @"recent.json";

    public Command ShowOpenedLogsCommand { get; private set; }
    public Command OpenSelectedAvailableRecentLogCommand { get; private set; }

    public MainViewModel MainViewModel
    {
      get { return this.mainViewModel; }
      private set
      {
        this.mainViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private MainViewModel mainViewModel;
    public ObservableCollection<RecentLogInfo> AvailableRecentLogs { get; set; }
    public RecentLogInfo? SelectedAvailableRecentLog
    {
      get { return this.selectedAvailableRecentLog; }
      set
      {
        this.selectedAvailableRecentLog = value;
        this.OnPropertyChanged();
      }
    }
    private RecentLogInfo? selectedAvailableRecentLog;

    public DashboardViewModel(MainViewModel mainViewModel) : this()
    {
      this.MainViewModel = mainViewModel;
    }
    public DashboardViewModel()
    {
      this.AvailableRecentLogs = new ObservableCollection<RecentLogInfo>();
      this.InitCommands();
    }

    public void InitCommands()
    {
      this.ShowOpenedLogsCommand = new Command(
        x => {
          if (this.mainViewModel != null)
            this.mainViewModel.SetAsActive(this.mainViewModel.LogManagerViewModel);
        },
        x => true
        );
      this.OpenSelectedAvailableRecentLogCommand = new Command(
        x => {
          if (this.MainViewModel == null ||
              !this.SelectedAvailableRecentLog.HasValue)
            return;
          this.MainViewModel.LogManagerViewModel.OpenLogs(this.SelectedAvailableRecentLog.Value.Path);
        },
        x => true
        );
    }

    public void RememberOpenedLogs()
    {
      if (this.MainViewModel.LogManagerViewModel == null)
        return;

      try
      {
        var logInfos = this.MainViewModel.LogManagerViewModel.Logs
          .Select(x => new RecentLogInfo()
          {
            DisplayName = x.DisplayName,
            Path = x.LogPath,
            LastOpened = System.DateTime.Now
          });

        logInfos = logInfos.Where(x => !this.AvailableRecentLogs.Any(y => y.Path == x.Path));
        logInfos = logInfos.Union(this.AvailableRecentLogs);
        File.WriteAllText(RecentFileName, JsonConvert.SerializeObject(logInfos, Formatting.Indented));
      }
      catch { }
    }

    public void FillAvailableRecentLogs()
    {
      try
      {
        if (!File.Exists(RecentFileName))
          return;
        var recentLogInfos = JsonConvert.DeserializeObject<List<RecentLogInfo>>(File.ReadAllText(RecentFileName));
        foreach (var logInfo in recentLogInfos)
        {
          if (!File.Exists(logInfo.Path) ||
              this.AvailableRecentLogs.Any(x => x.Path == logInfo.Path))
            continue;
          this.AvailableRecentLogs.Add(logInfo);
        }
      }
      catch { }
    }
  }
}
