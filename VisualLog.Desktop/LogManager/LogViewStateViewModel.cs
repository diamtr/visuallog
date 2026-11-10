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
    /// <summary>Width of the line details panel; kept while the panel is closed.</summary>
    public double DetailsPanelWidth
    {
      get { return this.detailsPanelWidth; }
      set {
        this.detailsPanelWidth = value;
        this.OnPropertyChanged();
      }
    }
    private double detailsPanelWidth = 420;

    public LogViewStateViewModel()
    {
      this.followTail = true;
      this.showSearchPanel = false;
    }
  }
}
