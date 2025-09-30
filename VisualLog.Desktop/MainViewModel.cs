using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VisualLog.Desktop.Dashboard;
using VisualLog.Desktop.FormatManager;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop
{
  public class MainViewModel : ViewModelBase
  {
    public ObservableCollection<LogViewModel> Logs { get; set; }

    public ViewModelBase ActiveViewModel
    {
      get { return this.activeViewModel; }
      private set { this.activeViewModel = value; this.OnPropertyChanged(); }
    }
    private ViewModelBase activeViewModel;

    public DashboardViewModel DashboardViewModel { get; private set; }
    public LogManagerViewModel LogManagerViewModel { get; private set; }
    public FormatManagerViewModel FormatManagerViewModel { get; private set; }
    public Command OpenLogsCommand { get; private set; }
    public Command ShowDashboardCommand { get; private set; }
    public Command ShowFormatManagerCommand { get; private set; }

    public MainViewModel()
    {
      this.Logs = new ObservableCollection<LogViewModel>();
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
    }

    public void SetAsActive(ViewModelBase viewModel)
    {
      this.ActiveViewModel = viewModel;
    }

    public void Open(params string[] paths)
    {
      if (paths == null)
        return;

      LogViewModel logViewModel = null;
      foreach (var path in paths)
        if (File.Exists(path))
        {
          logViewModel = this.Logs.FirstOrDefault(x => x.LogPath == path);
          if (logViewModel == null)
          {
            logViewModel = new LogViewModel(path);
            logViewModel.CloseRequested += this.OnLogCloseRequested;
            logViewModel.ShowRequested += this.OnLogShowRequested;
            logViewModel.ReadLog();
            logViewModel.FollowTail();
            this.Logs.Add(logViewModel);
          }
        }
      if (logViewModel != null)
        this.LogManagerViewModel.ActiveLog = logViewModel;
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
          this.Open(paths.ToArray());
          this.SetAsActive(this.LogManagerViewModel);
        },
        x => true
        );
      this.ShowDashboardCommand = new Command(
        x => { this.SetAsActive(this.DashboardViewModel); },
        x => true
        );
      this.ShowFormatManagerCommand = new Command(
        x => { this.SetAsActive(this.FormatManagerViewModel); },
        x => true
        );
    }

    private void OnLogCloseRequested(LogViewModel closedViewModel)
    {
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
        this.SetAsActive(this.DashboardViewModel);
    }

    private void OnLogShowRequested(LogViewModel logToShow)
    {
      this.LogManagerViewModel.ActiveLog = logToShow;
      this.SetAsActive(this.LogManagerViewModel);
    }
  }
}
