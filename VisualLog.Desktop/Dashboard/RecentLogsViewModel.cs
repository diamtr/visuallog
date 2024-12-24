using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace VisualLog.Desktop.Dashboard
{
  public class RecentLogsViewModel : ViewModelBase
  {
    public const string RecentFileName = @"recent.json";

    public Command OpenSelectedAvailableRecentLogCommand { get; private set; }
    public DashboardViewModel DashboardViewModel
    {
      get { return this.dashboardViewModel; }
      protected set
      {
        this.dashboardViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private DashboardViewModel dashboardViewModel;
    public ObservableCollection<RecentLogInfo> AvailableRecentLogs { get; set; }

    public RecentLogsViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.DashboardViewModel = dashboardViewModel;
    }

    public RecentLogsViewModel()
    {
      this.AvailableRecentLogs = new ObservableCollection<RecentLogInfo>();
      this.InitCommands();
    }

    public void InitCommands()
    {
      this.OpenSelectedAvailableRecentLogCommand = new Command(
        x => {
          var collectionParameter = x as IEnumerable<object>;
          if (collectionParameter == null)
            return;
          var selectedLogs = collectionParameter.Cast<RecentLogInfo>();
          if (this.DashboardViewModel == null ||
              this.DashboardViewModel.MainViewModel == null)
            return;
          this.DashboardViewModel.MainViewModel.LogManagerViewModel.OpenLogs(selectedLogs.Select(x => x.Path).ToArray());
        },
        x => true
        );
    }

    public void RememberOpenedLogs()
    {
      if (this.DashboardViewModel == null ||
          this.DashboardViewModel.MainViewModel == null ||
          this.DashboardViewModel.MainViewModel.LogManagerViewModel == null)
        return;

      try
      {
        var logInfos = this.DashboardViewModel.MainViewModel.LogManagerViewModel.Logs
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
