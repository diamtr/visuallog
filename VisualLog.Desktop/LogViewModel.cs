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

    private Log log;
    private string selectedEncoding;

    public LogViewModel()
    {
      this.LogMessages = new List<string>();
      this.Encodings = new List<string>();
    }

    public void ReadLog(string path)
    {
      if (string.IsNullOrWhiteSpace(path) ||
          !System.IO.File.Exists(path))
        return;

      var encs = Encoding.GetEncodings();
      this.Encodings.AddRange(encs.Select(x => this.GetEncodingDisplayName(x.GetEncoding())));
      this.OnPropertyChanged(nameof(this.Encodings));
      this.SelectedEncoding = this.GetEncodingDisplayName(Encoding.UTF8);
      this.log = new Log();
      this.log.Read(path);
      this.LogMessages = this.log.Messages.Select(x => x.RawValue).ToList();
      this.OnPropertyChanged(nameof(this.LogMessages));
    }

    private string GetEncodingDisplayName(Encoding encoding)
    {
      return $"{encoding.CodePage} {encoding.WebName} {encoding.EncodingName}";
    }
  }
}
