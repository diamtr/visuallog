namespace VisualLog.Desktop
{
  public class MainViewModel
  {
    public Command OpenFileCommand { get; private set; }

    public MainViewModel()
    {
      this.InitCommands();
    }

    private void InitCommands()
    {
      this.OpenFileCommand = new Command(
        x => { },
        x => true
        );
    }
  }
}
