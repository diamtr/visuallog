using Microsoft.Win32;
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
    public LogViewModel LogViewModel { get; private set; }
    public LogManagerViewModel LogManagerViewModel { get; private set; }
    public FormatManagerViewModel FormatManagerViewModel { get; private set; }
    public MainViewModelContext Context { get; private set; }
    public Command OpenFileCommand { get; private set; }
    public Command ShowLogManagerCommand { get; private set; }
    public Command ShowFormatManagerCommand { get; private set; }

    private ViewModelBase activeViewModel;

    public MainViewModel()
    {
      this.Context = new MainViewModelContext();
      this.LogViewModel = new LogViewModel();
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
      this.OpenFileCommand = new Command(
        x =>
        {
          var path = OpenFile();
          this.LogViewModel.ReadLog(path);
          this.Context.SetAsActive(this.LogViewModel);
        },
        x => true
        );

      this.ShowLogManagerCommand = new Command(
        x =>
        {
          this.Context.SetAsActive(this.LogManagerViewModel);
        },
        x => true
        );

      this.ShowFormatManagerCommand = new Command(
        x =>
        {
          this.Context.SetAsActive(this.FormatManagerViewModel);
        },
        x => true
        );
    }

    public static string OpenFile()
    {
      var dialog = new OpenFileDialog();
      dialog.Multiselect = false;

      if (dialog.ShowDialog() != true)
        return string.Empty;

      return dialog.FileName;
    }
  }
}
