using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualLog.Core;

namespace VisualLog.Desktop
{
  public class LogViewModel : ViewModelBase
  {
    public List<string> LogMessages { get; set; }
    public List<string> Encodings { get; set; }
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

    private string logPath;
    private Log log;
    private string selectedEncoding;

    public LogViewModel()
    {
      this.LogMessages = new List<string>();
      this.Encodings = new List<string>();
      this.InitEncodings();
      this.PropertyChanged += SelectedEncoding_PropertyChanged;
    }

    public void ReadLog()
    {
      if (string.IsNullOrWhiteSpace(this.logPath) ||
          !System.IO.File.Exists(this.logPath))
        return;

      this.ReadLog(this.logPath);
    }

    public void ReadLog(string path)
    {
      if (string.IsNullOrWhiteSpace(path) ||
          !System.IO.File.Exists(path))
        return;

      this.logPath = path;
      var encoding = Encoding.Default;
      if (!string.IsNullOrWhiteSpace(this.SelectedEncoding))
        encoding = Encoding.GetEncoding(int.Parse(this.SelectedEncoding.Split(' ')[0]));
      this.log = new Log(encoding);
      this.log.Read(this.logPath);
      this.LogMessages = this.log.Messages.Select(x => x.RawValue).ToList();
      this.OnPropertyChanged(nameof(this.LogMessages));
    }

    private void InitEncodings()
    {
      var encodings = Encoding.GetEncodings();
      this.Encodings.AddRange(encodings.Select(x => this.GetEncodingDisplayName(x.GetEncoding())));
      this.OnPropertyChanged(nameof(this.Encodings));
      this.SelectedEncoding = this.GetEncodingDisplayName(Encoding.UTF8);
    }

    private string GetEncodingDisplayName(Encoding encoding)
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
