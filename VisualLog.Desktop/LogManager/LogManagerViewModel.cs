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
            logViewModel.ReadLog();
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
          string[] paths = null;
          if (x != null && 
              x is string && 
              (string)x == "LogManagerView")
          {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
              paths = dialog.FileNames;
          }
          this.OpenLogs(paths);
        },
        x => true
        );
    }
  }
}
