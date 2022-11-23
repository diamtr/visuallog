﻿using System.Collections.Generic;
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
    public bool ShowFormattedMessageVertical
    {
      get
      {
        return this.showFormattedMessageVertical;
      }
      set
      {
        this.showFormattedMessageVertical = value;
        if (this.showFormattedMessageVertical && this.showFormattedMessageHorizontal)
          this.ShowFormattedMessageHorizontal = false;
        this.OnPropertyChanged();
      }
    }
    public bool ShowFormattedMessageHorizontal
    {
      get
      {
        return this.showFormattedMessageHorizontal;
      }
      set
      {
        this.showFormattedMessageHorizontal = value;
        if (this.showFormattedMessageHorizontal && this.showFormattedMessageVertical)
          this.ShowFormattedMessageVertical = false;
        this.OnPropertyChanged();
      }
    }
    public List<Format> LogFormats { get; set; }

    private string displayName;
    private string logPath;
    private Log log;
    private bool followLogTail;
    private string selectedEncoding;
    private bool showFormattedMessageVertical;
    private bool showFormattedMessageHorizontal;

    public LogViewModel(string path) : this()
    {
      this.LogPath = path;
    }
    
    public LogViewModel()
    {
      this.LogMessages = new ObservableCollection<MessageInlineViewModel>();
      this.Encodings = new List<string>();
      this.LogFormats = new List<Format>();
      this.InitEncodings();
      this.PropertyChanged += SelectedEncoding_PropertyChanged;
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
      if (this.followLogTail)
      {
        this.log.CatchNewMessage += this.OnNewLogMessageCatched;
        this.log.StartFollowTail();
      }
    }

    public void FollowTail()
    {
      this.followLogTail = true;
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
