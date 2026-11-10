namespace VisualLog.Desktop.LogManager
{
  public class LogViewStateViewModel : ViewModelBase
  {
    public bool ShowSearchPanel
    {
      get { return this.showSearchPanel; }
      set {
        this.showSearchPanel = value;
        this.OnPropertyChanged();
      }
    }
    private bool showSearchPanel;
    public bool ShowSelectedMessagesPanel
    {
      get { return this.showSelectedMessagesVertical; }
      set {
        this.showSelectedMessagesVertical = value;
        this.OnPropertyChanged();
      }
    }
    private bool showSelectedMessagesVertical;
    public bool FollowTail
    {
      get { return this.followTail; }
      set {
        this.followTail = value;
        this.OnPropertyChanged();
      }
    }
    private bool followTail;
    /// <summary>Height of the search results panel; kept while the panel is closed.</summary>
    public double SearchPanelHeight
    {
      get { return this.searchPanelHeight; }
      set {
        this.searchPanelHeight = value;
        this.OnPropertyChanged();
      }
    }
    private double searchPanelHeight = 240;

    public LogViewStateViewModel()
    {
      this.followTail = true;
      this.showSearchPanel = false;
      this.showSelectedMessagesVertical = false;
    }
  }
}
