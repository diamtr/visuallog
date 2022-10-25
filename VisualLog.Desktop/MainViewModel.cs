using VisualLog.Desktop.FormatManager;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop
{
  public class MainViewModel : ViewModelBase
  {
    public ViewModelBase ActiveViewModel
    {
      get { return this.activeViewModel; }
      private set { this.activeViewModel = value; this.OnPropertyChanged(); }
    }
    public LogManagerViewModel LogManagerViewModel { get; private set; }
    public FormatManagerViewModel FormatManagerViewModel { get; private set; }
    public Command ShowLogManagerCommand { get; private set; }
    public Command ShowFormatManagerCommand { get; private set; }

    private ViewModelBase activeViewModel;

    public MainViewModel()
    {
      this.LogManagerViewModel = new LogManagerViewModel();
      this.FormatManagerViewModel = new FormatManagerViewModel();
      this.InitCommands();
    }

    public void OnWindowLoaded()
    {
      this.SetAsActive(this.LogManagerViewModel);
    }

    public void SetAsActive(ViewModelBase viewModel)
    {
      this.ActiveViewModel = viewModel;
    }

    private void InitCommands()
    {
      this.ShowLogManagerCommand = new Command(
        x =>
        {
          this.SetAsActive(this.LogManagerViewModel);
        },
        x => true
        );

      this.ShowFormatManagerCommand = new Command(
        x =>
        {
          this.SetAsActive(this.FormatManagerViewModel);
        },
        x => true
        );
    }
  }
}
