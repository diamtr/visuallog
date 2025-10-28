using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Dashboard
{
  public class RecentLogsViewModel : ViewModelBase
  {
    public const string RecentFileName = @"recent.json";

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
    public ObservableCollection<RecentLogViewModel> AvailableRecentLogs { get; set; }

    public RecentLogsViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.DashboardViewModel = dashboardViewModel;
    }

    public RecentLogsViewModel()
    {
      this.AvailableRecentLogs = new ObservableCollection<RecentLogViewModel>();
    }

    public void Open(string path)
    {
      this.DashboardViewModel.MainViewModel.Open(path);
    }

    public void RememberOpenedLogs()
    {
      if (this.DashboardViewModel == null ||
          this.DashboardViewModel.MainViewModel == null ||
          this.DashboardViewModel.MainViewModel.LogManagerViewModel == null)
        return;

      try
      {
        var logInfos = this.DashboardViewModel.MainViewModel.Logs
          .Select(x => new RecentLogViewModel()
          {
            DisplayName = x.DisplayName,
            Path = x.LogPath,
            LastOpened = DateTime.Now
          });

        logInfos = logInfos.Where(x => !this.AvailableRecentLogs.Any(y => y.Path == x.Path));
        logInfos = logInfos.Union(this.AvailableRecentLogs);
        File.WriteAllText(RecentFileName, JsonConvert.SerializeObject(logInfos, Formatting.Indented));
      }
      catch { }
    }

    public void RememberLog(LogViewModel logViewModel)
    {
      var recentLogViewModel = new RecentLogViewModel()
      {
        DisplayName = logViewModel.DisplayName,
        Path = logViewModel.LogPath,
        LastOpened = DateTime.Now
      };

      var original = this.AvailableRecentLogs.FirstOrDefault(x => Equals(x, recentLogViewModel));

      if (original == null)
        this.AvailableRecentLogs.Add(recentLogViewModel);
      else
        original.LastOpened = DateTime.Now;
    }

    public void FillAvailableRecentLogs()
    {
      try
      {
        if (!File.Exists(RecentFileName))
          return;
        var recentLogInfos = JsonConvert.DeserializeObject<List<RecentLogViewModel>>(File.ReadAllText(RecentFileName));
        foreach (var logInfo in recentLogInfos)
        {
          if (!File.Exists(logInfo.Path) ||
              this.AvailableRecentLogs.Any(x => x.Path == logInfo.Path))
            continue;
          logInfo.Parent = this;
          this.AvailableRecentLogs.Add(logInfo);
        }
      }
      catch { }
    }
  }
}
