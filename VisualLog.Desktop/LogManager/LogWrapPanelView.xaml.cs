using System.Windows.Controls;
using System.Windows.Input;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// A tab of the opened logs strip.
  /// </summary>
  public partial class LogWrapPanelView : UserControl
  {
    public LogWrapPanelView()
    {
      InitializeComponent();
    }

    private void Tab_MouseUp(object sender, MouseButtonEventArgs e)
    {
      // Middle click closes the tab, like in browsers and editors
      if (e.ChangedButton != MouseButton.Middle)
        return;
      if (this.DataContext is LogViewModel log && log.CloseCommand.CanExecute(null))
      {
        log.CloseCommand.Execute(null);
        e.Handled = true;
      }
    }
  }
}
