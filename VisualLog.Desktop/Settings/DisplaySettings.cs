namespace VisualLog.Desktop.Settings
{
  /// <summary>
  /// Application-wide look of the opened logs. One instance is shared by the settings page
  /// and every log view, so a change shows up in all opened logs at once.
  /// </summary>
  public class DisplaySettings : ViewModelBase
  {
    public bool ShowLineNumbers
    {
      get { return this.showLineNumbers; }
      set
      {
        if (this.showLineNumbers == value)
          return;
        this.showLineNumbers = value;
        this.OnPropertyChanged();
      }
    }
    private bool showLineNumbers = AppSettings.DefaultShowLineNumbers;
  }
}
