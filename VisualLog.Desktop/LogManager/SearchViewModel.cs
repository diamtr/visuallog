using System;

namespace VisualLog.Desktop.LogManager
{
  public class SearchViewModel : ViewModelBase
  {
    public Command HideSearchPanelCommand { get; private set; }
    public Command SearchCommand { get; private set; }
    public string StringToSearch { get; set; }
    public event Action HideSearchPanelRequested;
    public event EventHandler<string> SearchRequested;

    public SearchViewModel()
    {
      this.InitCommands();
    }

    private void InitCommands()
    {
      this.HideSearchPanelCommand = new Command(
        x =>
        {
          if (this.HideSearchPanelRequested != null)
            this.HideSearchPanelRequested.Invoke();
        },
        x => true
      );
      this.SearchCommand = new Command(
        x =>
        {
          if (this.SearchRequested != null)
            this.SearchRequested.Invoke(this, this.StringToSearch);
        },
        x => true
      );
    }
  }
}
