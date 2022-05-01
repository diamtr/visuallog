using Microsoft.Win32;

namespace VisualLog.Desktop
{
  public class MainViewModel : ViewModelBase
  {
    public LogViewModel LogViewModel { get; private set; }
    public Command OpenFileCommand { get; private set; }
    public Command ShowFormatManagerCommand { get; private set; }

    public MainViewModel()
    {
      this.LogViewModel = new LogViewModel();
      this.InitCommands();
    }

    private void InitCommands()
    {
      this.OpenFileCommand = new Command(
        x =>
        {
          var path = OpenFile();
          this.LogViewModel.ReadLog(path);
        },
        x => true
        );

      this.ShowFormatManagerCommand = new Command(
        x =>
        {

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
