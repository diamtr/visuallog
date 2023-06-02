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

    private bool showSearchPanel;
    private bool showSelectedMessagesVertical;
    private bool showSelectedMessagesHorizontal;
    private bool followTail;

    public LogViewStateViewModel()
    {
      this.showSearchPanel = false;
      this.followTail = true;
    }
  }
}
