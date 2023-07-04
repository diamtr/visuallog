using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class LogViewModel : ViewModelBase
  {
    public Command ShowSearchPanelCommand { get; private set; }
    public ObservableCollection<MessageInlineViewModel> LogMessages { get; set; }
    public string DisplayName
    {
      get
      {
        return this.displayName;
      }
      set
      {
        this.displayName = value;
        this.OnPropertyChanged();
      }
    }
    private string displayName;
    public List<string> Encodings { get; set; }
    public string LogPath
    {
      get
      {
        return this.logPath;
      }
      set
      {
        this.logPath = value;
        this.OnPropertyChanged();
        this.DisplayName = Path.GetFileName(this.logPath);
      }
    }
    private string logPath;
    public string SelectedEncoding
    {
      get
      {
        return this.selectedEncoding;
      }
      set
      {
        this.selectedEncoding = value;
        this.OnPropertyChanged();
      }
    }
    private string selectedEncoding;
    public List<Format> LogFormats { get; set; }
    public LogViewStateViewModel State { get; set; }
    public SearchViewModel SearchViewModel { get; set; }
    

    private Log log;

    public LogViewModel(string path) : this()
    {
      this.LogPath = path;
    }
    
    public LogViewModel()
    {
      this.LogMessages = new ObservableCollection<MessageInlineViewModel>();
      this.Encodings = new List<string>();
      this.LogFormats = new List<Format>();
      this.State = new LogViewStateViewModel();
      this.SearchViewModel = new SearchViewModel();
      this.SearchViewModel.HideSearchPanelRequested += SearchViewModel_HideSearchPanelRequested;
      this.SearchViewModel.SearchRequested += SearchViewModel_SearchRequested;
      this.InitEncodings();
      this.PropertyChanged += SelectedEncoding_PropertyChanged;
      this.InitCommands();
    }

    private void SearchViewModel_SearchRequested(object sender, string e)
    {
      throw new System.NotImplementedException();
    }

    private void SearchViewModel_HideSearchPanelRequested()
    {
      this.State.ShowSearchPanel = false;
    }

    private void InitCommands()
    {
      this.ShowSearchPanelCommand = new Command(
        x => this.State.ShowSearchPanel = true,
        x => true
      );
    }

    public void ReadLog()
    {
      if (string.IsNullOrWhiteSpace(this.logPath) ||
          !File.Exists(this.logPath))
        return;

      this.ReadLog(this.logPath);
    }

    public void ReadLog(string path)
    {
      if (string.IsNullOrWhiteSpace(path) ||
          !File.Exists(path))
        return;

      this.logPath = path;
      var encoding = Encoding.Default;
      if (!string.IsNullOrWhiteSpace(this.SelectedEncoding))
        encoding = Encoding.GetEncoding(int.Parse(this.SelectedEncoding.Split(' ')[0]));
      this.log = new Log(this.logPath, encoding);
      this.log.Read();
      this.LogMessages.Clear();
      foreach (var message in this.log.Messages)
        this.LogMessages.Add(new MessageInlineViewModel(message));
      if (this.State.FollowTail)
      {
        this.log.CatchNewMessage += this.OnNewLogMessageCatched;
        this.log.StartFollowTail();
      }
    }

    public void FollowTail()
    {
      this.State.FollowTail = true;
      if (this.log == null)
        return;
      this.log.CatchNewMessage += this.OnNewLogMessageCatched;
      this.log.StartFollowTail();
    }

    public void OnNewLogMessageCatched(Message message)
    {
      if (message == null)
        return;
      try
      {
        // UI thread safety
        if (Application.Current != null)
          Application.Current.Dispatcher.Invoke(() => { this.LogMessages.Add(new MessageInlineViewModel(message)); });
        else
          this.LogMessages.Add(new MessageInlineViewModel(message));
      }
      catch
      {
      }
    }

    public void InitEncodings()
    {
      var encodings = Encoding.GetEncodings();
      this.Encodings.AddRange(encodings.Select(x => this.GetEncodingDisplayName(x.GetEncoding())));
      this.OnPropertyChanged(nameof(this.Encodings));
      this.SelectedEncoding = this.GetEncodingDisplayName(Encoding.UTF8);
    }

    public string GetEncodingDisplayName(Encoding encoding)
    {
      return $"{encoding.CodePage} {encoding.WebName} {encoding.EncodingName}";
    }

    private void SelectedEncoding_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (sender == null || e.PropertyName != nameof(SelectedEncoding))
        return;

      this.ReadLog();
    }
  }
}
