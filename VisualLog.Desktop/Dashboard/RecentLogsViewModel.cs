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

    private DashboardViewModel dashboardViewModel;
    public DashboardViewModel DashboardViewModel
    {
      get { return this.dashboardViewModel; }
      protected set
      {
        this.dashboardViewModel = value;
        this.OnPropertyChanged();
      }
    }

    public ObservableCollection<RecentLogViewModel> RecentLogs { get; set; }

    protected List<RecentLogInfo> recentLogInfos;
    

    public RecentLogsViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.DashboardViewModel = dashboardViewModel;
    }

    public RecentLogsViewModel()
    {
      this.recentLogInfos = new List<RecentLogInfo>();
      this.RecentLogs = new ObservableCollection<RecentLogViewModel>();
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
          .Select(x => new RecentLogInfo()
          {
            DisplayName = x.DisplayName,
            Path = x.LogPath,
            LastOpened = DateTime.Now
          });

        logInfos = logInfos.Where(x => !this.recentLogInfos.Any(y => y.Path == x.Path));
        logInfos = logInfos.Union(this.recentLogInfos);
        logInfos = logInfos.OrderByDescending(x => x.LastOpened);
        File.WriteAllText(RecentFileName, JsonConvert.SerializeObject(logInfos, Formatting.Indented));
      }
      catch { }
    }

    public void RememberLog(LogViewModel logViewModel)
    {
      var recentLogInfo = new RecentLogInfo()
      {
        DisplayName = logViewModel.DisplayName,
        Path = logViewModel.LogPath,
        LastOpened = DateTime.Now
      };

      var original = this.recentLogInfos.FirstOrDefault(x => Equals(x, recentLogInfo));
      if (original == null)
        this.recentLogInfos.Add(recentLogInfo);
      else
        original.LastOpened = DateTime.Now;

      this.RefreshRecentLogs();
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
              this.recentLogInfos.Any(x => x.Path == logInfo.Path))
            continue;
          this.recentLogInfos.Add(logInfo);
        }

        this.RefreshRecentLogs();
      }
      catch { }
    }

    protected void RefreshRecentLogs()
    {
      var logInfos = this.recentLogInfos.OrderByDescending(x => x.LastOpened);
      this.RecentLogs.Clear();
      foreach (var info in logInfos)
        this.RecentLogs.Add(new RecentLogViewModel(info));
    }
  }
}
