namespace VisualLog.Desktop.LogManager
{
  public class LogManagerStateViewModel : ViewModelBase
  {
    private bool showSearchPanel;
    public bool ShowSearchPanel
    {
      get
      {
        return this.showSearchPanel;
      }
      set
      {
        this.showSearchPanel = value;
        this.OnPropertyChanged();
      }
    }

    public LogManagerStateViewModel()
    {
      this.showSearchPanel = false;
    }
  }
}
