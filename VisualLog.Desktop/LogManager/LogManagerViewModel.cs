using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace VisualLog.Desktop.LogManager
{
  public class LogManagerViewModel : ViewModelBase
  {
    public Command OpenLogsCommand { get; private set; }
    public Command ShowSearchPanelCommand { get; private set; }
    public ObservableCollection<LogViewModel> Logs { get; set; }
    
    public LogViewModel ActiveLog
    { 
      get
      {
        return this.activeLog;
      }
      set
      {
        this.activeLog = value;
        this.OnPropertyChanged();
      }
    }
    private LogViewModel activeLog;

    public MainViewModel MainViewModel
    {
      get { return this.mainViewModel; }
      protected set
      {
        this.mainViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private MainViewModel mainViewModel;

    public LogManagerViewModel(MainViewModel mainViewModel) : this()
    {
      this.MainViewModel = mainViewModel;
    }
    public LogManagerViewModel()
    {
      this.Logs = new ObservableCollection<LogViewModel>();
      this.InitCommands();
    }

    public void OpenLogs(params string[] paths)
    {
      if (paths == null)
        return;

      foreach (var path in paths)
        if (File.Exists(path))
        {
          var logViewModel = this.Logs.FirstOrDefault(x => x.LogPath == path);
          if (logViewModel == null)
          {
            logViewModel = new LogViewModel(path);
            logViewModel.CloseRequested += this.OnLogCloseRequested;
            logViewModel.ReadLog();
            logViewModel.FollowTail();
            this.Logs.Add(logViewModel);
          }
          this.ActiveLog = logViewModel;
        }
    }

    private void InitCommands()
    {
      this.OpenLogsCommand = new Command(
        x =>
        {
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
          this.OpenLogs(paths.ToArray());
        },
        x => true
        );
    }

    private void OnLogCloseRequested(LogViewModel logViewModel)
    {
      this.Logs.Remove(logViewModel);
      if (Equals(this.ActiveLog, logViewModel))
        this.ActiveLog = this.Logs.FirstOrDefault();
    }
  }
}
