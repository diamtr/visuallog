using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VisualLog.Desktop.Dashboard;
using VisualLog.Desktop.FormatManager;
using VisualLog.Desktop.LogManager;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop
{
  public class MainViewModel : ViewModelBase
  {
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    public ObservableCollection<LogViewModel> Logs { get; set; }

    public ViewModelBase ActiveViewModel
    {
      get { return this.activeViewModel; }
      private set
      {
        this.activeViewModel = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.IsDashboardActive));
        this.OnPropertyChanged(nameof(this.IsLogManagerActive));
        this.OnPropertyChanged(nameof(this.IsSettingsActive));
      }
    }
    private ViewModelBase activeViewModel;

    public bool IsDashboardActive => this.ActiveViewModel != null && this.ActiveViewModel == this.DashboardViewModel;
    public bool IsLogManagerActive => this.ActiveViewModel != null && this.ActiveViewModel == this.LogManagerViewModel;
    public bool IsSettingsActive => this.ActiveViewModel != null && this.ActiveViewModel == this.SettingsViewModel;

    public DashboardViewModel DashboardViewModel { get; private set; }
    public LogManagerViewModel LogManagerViewModel { get; private set; }
    public FormatManagerViewModel FormatManagerViewModel { get; private set; }
    public SettingsViewModel SettingsViewModel { get; private set; }
    /// <summary>Current line highlighting rules, shared by all opened logs.</summary>
    public HighlightingService Highlighting { get; private set; }
    public Command OpenLogsCommand { get; private set; }
    public Command ShowDashboardCommand { get; private set; }
    public Command ShowLogsCommand { get; private set; }
    public Command ShowFormatManagerCommand { get; private set; }
    public Command ShowSettingsCommand { get; private set; }

    public MainViewModel()
    {
      this.Logs = new ObservableCollection<LogViewModel>();
      this.Highlighting = new HighlightingService();
      // Before any log is opened, so logs start with the saved highlighting
      this.SettingsViewModel = new SettingsViewModel(this.Highlighting);
      this.SettingsViewModel.Load();
      this.DashboardViewModel = new DashboardViewModel(this);
      this.LogManagerViewModel = new LogManagerViewModel(this);
      this.FormatManagerViewModel = new FormatManagerViewModel();
      this.InitCommands();
    }

    public void OnWindowLoaded()
    {
      this.SetAsActive(this.DashboardViewModel);
    }

    public void OnWindowClosing()
    {
      if (this.DashboardViewModel != null)
        this.DashboardViewModel.RememberOpenedLogs();
      this.SettingsViewModel?.ApplyPending();
    }

    public void SetAsActive(ViewModelBase viewModel)
    {
      this.ActiveViewModel = viewModel;
    }

    /// <summary>
    /// Opens logs (or activates already opened ones) and remembers them as recent.
    /// Returns the log that became active, or null if nothing could be opened.
    /// </summary>
    public LogViewModel Open(params string[] paths)
    {
      if (paths == null)
        return null;

      LogViewModel logViewModel = null;
      foreach (var path in paths)
      {
        if (!File.Exists(path))
        {
          log.Warn($"Open: file not found '{path}'");
          continue;
        }

        logViewModel = this.Logs.FirstOrDefault(x => x.LogPath == path);
        if (logViewModel != null)
        {
          log.Debug($"Open: already opened, activate '{path}'");
        }
        else
        {
          log.Info($"Open: '{path}', {new FileInfo(path).Length:N0} bytes");
          logViewModel = new LogViewModel(path);
          logViewModel.Highlighting = this.Highlighting;
          logViewModel.CloseRequested += this.OnLogCloseRequested;
          logViewModel.ShowRequested += this.OnLogShowRequested;
          logViewModel.ReadLog();
          logViewModel.FollowTail();
          this.Logs.Add(logViewModel);
          log.Debug($"Open: added to opened logs, {this.Logs.Count} opened");
        }
        this.DashboardViewModel?.RememberLog(logViewModel);
      }
      if (logViewModel != null)
      {
        this.LogManagerViewModel.ActiveLog = logViewModel;
        log.Debug($"Open: active log '{logViewModel.LogPath}'");
      }
      return logViewModel;
    }

    /// <summary>
    /// Opens logs and switches to the log view if at least one of them was opened.
    /// </summary>
    public void OpenAndShow(params string[] paths)
    {
      if (this.Open(paths) != null)
        this.SetAsActive(this.LogManagerViewModel);
    }

    private void InitCommands()
    {
      this.OpenLogsCommand = new Command(
        x => {
          List<string> paths = new List<string>();
          if (x is not null && x is string)
          {
            paths.Add((string)x);
          }
          else
          {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
              paths = dialog.FileNames.ToList();
          }
          if (paths.Any())
            this.OpenAndShow(paths.ToArray());
        },
        x => true
        );
      this.ShowDashboardCommand = new Command(
        x => { this.SetAsActive(this.DashboardViewModel); },
        x => true
        );
      this.ShowLogsCommand = new Command(
        x => { this.SetAsActive(this.LogManagerViewModel); },
        x => this.Logs.Any()
        );
      this.ShowFormatManagerCommand = new Command(
        x => { this.SetAsActive(this.FormatManagerViewModel); },
        x => true
        );
      this.ShowSettingsCommand = new Command(
        x => { this.SetAsActive(this.SettingsViewModel); },
        x => true
        );
    }

    private void OnLogCloseRequested(LogViewModel closedViewModel)
    {
      log.Info($"Close: '{closedViewModel?.LogPath}'");
      this.DashboardViewModel.RememberLog(closedViewModel);

      if (!Equals(this.LogManagerViewModel.ActiveLog, closedViewModel))
      {
        this.Logs.Remove(closedViewModel);
        closedViewModel.Dispose();
        return;
      }

      var closedViewModelIndex = this.Logs.IndexOf(closedViewModel);
      var nearestViewModelIndex = -1;
      if (this.Logs.Count > 1)
      {
        if (closedViewModelIndex < this.Logs.Count - 1)
          nearestViewModelIndex = closedViewModelIndex;
        else
          nearestViewModelIndex = closedViewModelIndex - 1;
      }
      this.Logs.Remove(closedViewModel);
      closedViewModel.Dispose();
      if (nearestViewModelIndex >= 0 && this.Logs.Any())
        this.LogManagerViewModel.ActiveLog = this.Logs[nearestViewModelIndex];
      if (!this.Logs.Any())
      {
        this.LogManagerViewModel.ActiveLog = null;
        this.SetAsActive(this.DashboardViewModel);
      }
    }

    private void OnLogShowRequested(LogViewModel logToShow)
    {
      this.LogManagerViewModel.ActiveLog = logToShow;
      this.SetAsActive(this.LogManagerViewModel);
    }
  }
}
