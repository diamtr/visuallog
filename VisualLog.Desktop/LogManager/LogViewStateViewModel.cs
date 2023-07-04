namespace VisualLog.Desktop.LogManager
{
  public class LogViewStateViewModel : ViewModelBase
  {
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
    private bool showSearchPanel;
    public bool ShowSelectedMessageVertical
    {
      get
      {
        return this.showSelectedMessagesVertical;
      }
      set
      {
        this.showSelectedMessagesVertical = value;
        if (this.showSelectedMessagesVertical && this.showSelectedMessagesHorizontal)
          this.showSelectedMessagesHorizontal = false;
        this.OnPropertyChanged();
      }
    }
    private bool showSelectedMessagesVertical;
    public bool ShowSelectedMessageHorizontal
    {
      get
      {
        return this.showSelectedMessagesHorizontal;
      }
      set
      {
        this.showSelectedMessagesHorizontal = value;
        if (this.showSelectedMessagesHorizontal && this.showSelectedMessagesVertical)
          this.showSelectedMessagesVertical = false;
        this.OnPropertyChanged();
      }
    }
    private bool showSelectedMessagesHorizontal;
    public bool FollowTail
    {
      get
      {
        return this.followTail;
      }
      set
      {
        this.followTail = value;
        this.OnPropertyChanged();
      }
    }
    private bool followTail;

    public LogViewStateViewModel()
    {
      this.followTail = true;
      this.showSearchPanel = false;
    }
  }
}
