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
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    public const string RecentFileName = @"recent.json";

    /// <summary>How many recent logs are shown on the dashboard.</summary>
    public const int MaxDisplayed = 10;

    /// <summary>How many recent logs are kept in the storage file.</summary>
    public const int MaxStored = 50;

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

    /// <summary>
    /// Full path of the storage file. Resolved against the application directory,
    /// so the history does not depend on the current working directory.
    /// </summary>
    public string StoragePath { get; set; } = Path.Combine(AppContext.BaseDirectory, RecentFileName);

    public ObservableCollection<RecentLogViewModel> RecentLogs { get; set; }

    public Command ClearCommand { get; private set; }

    protected List<RecentLogInfo> recentLogInfos;
    private bool isLoaded;

    public RecentLogsViewModel(DashboardViewModel dashboardViewModel) : this()
    {
      this.DashboardViewModel = dashboardViewModel;
    }

    public RecentLogsViewModel()
    {
      this.recentLogInfos = new List<RecentLogInfo>();
      this.RecentLogs = new ObservableCollection<RecentLogViewModel>();
      this.ClearCommand = new Command(x => this.Clear(), x => this.recentLogInfos.Any());
    }

    public void Open(string path)
    {
      if (!File.Exists(path))
      {
        // The file was moved or deleted: there is no point to keep it in the list.
        this.Forget(path);
        return;
      }
      this.DashboardViewModel?.MainViewModel?.OpenAndShow(path);
    }

    public void Forget(string path)
    {
      this.EnsureLoaded();
      if (this.recentLogInfos.RemoveAll(x => SamePath(x.Path, path)) == 0)
        return;
      this.RefreshRecentLogs();
      this.Save();
    }

    public void Clear()
    {
      this.EnsureLoaded();
      this.recentLogInfos.Clear();
      this.RefreshRecentLogs();
      this.Save();
    }

    public void RememberOpenedLogs()
    {
      var logs = this.DashboardViewModel?.MainViewModel?.Logs;
      if (logs == null || !logs.Any())
        return;

      this.EnsureLoaded();
      foreach (var log in logs)
        this.Upsert(log.LogPath, log.DisplayName);
      this.Save();
    }

    public void RememberLog(LogViewModel logViewModel)
    {
      if (logViewModel == null)
        return;
      this.Remember(logViewModel.LogPath, logViewModel.DisplayName);
    }

    public void Remember(string path, string displayName = null)
    {
      if (string.IsNullOrEmpty(path))
        return;

      this.EnsureLoaded();
      this.Upsert(path, displayName ?? Path.GetFileName(path));
      this.RefreshRecentLogs();
      this.Save();
    }

    public void FillAvailableRecentLogs()
    {
      this.EnsureLoaded();
      this.RefreshRecentLogs();
    }

    protected void Upsert(string path, string displayName)
    {
      var original = this.recentLogInfos.FirstOrDefault(x => SamePath(x.Path, path));
      if (original == null)
      {
        this.recentLogInfos.Add(new RecentLogInfo()
        {
          DisplayName = displayName,
          Path = path,
          LastOpened = DateTime.Now
        });
      }
      else
      {
        original.DisplayName = displayName;
        original.LastOpened = DateTime.Now;
      }
    }

    /// <summary>
    /// Loads the storage file once. Must be called before any modification,
    /// otherwise saving would overwrite the history that was not read yet.
    /// </summary>
    protected void EnsureLoaded()
    {
      if (this.isLoaded)
        return;
      this.isLoaded = true;

      try
      {
        if (!File.Exists(this.StoragePath))
          return;
        var stored = JsonConvert.DeserializeObject<List<RecentLogInfo>>(File.ReadAllText(this.StoragePath));
        if (stored == null)
          return;
        foreach (var logInfo in stored)
        {
          if (string.IsNullOrEmpty(logInfo?.Path) ||
              !File.Exists(logInfo.Path) ||
              this.recentLogInfos.Any(x => SamePath(x.Path, logInfo.Path)))
            continue;
          this.recentLogInfos.Add(logInfo);
        }
      }
      catch (Exception ex)
      {
        log.Error(ex, $"Recent logs could not be read from '{this.StoragePath}'");
      }
    }

    protected void Save()
    {
      try
      {
        var toStore = this.recentLogInfos
          .OrderByDescending(x => x.LastOpened)
          .Take(MaxStored)
          .ToList();
        File.WriteAllText(this.StoragePath, JsonConvert.SerializeObject(toStore, Formatting.Indented));
      }
      catch (Exception ex)
      {
        log.Error(ex, $"Recent logs could not be saved to '{this.StoragePath}'");
      }
    }

    protected void RefreshRecentLogs()
    {
      var logInfos = this.recentLogInfos
        .OrderByDescending(x => x.LastOpened)
        .Take(MaxDisplayed);
      this.RecentLogs.Clear();
      foreach (var info in logInfos)
        this.RecentLogs.Add(new RecentLogViewModel(info, this));
    }

    private static bool SamePath(string a, string b)
    {
      return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
  }
}
