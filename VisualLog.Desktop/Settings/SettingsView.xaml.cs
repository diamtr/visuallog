using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace VisualLog.Desktop.Settings
{
  public partial class SettingsView : UserControl
  {
    public SettingsView()
    {
      InitializeComponent();
    }

    // A just added rule gets the keyboard focus in its pattern box
    private void PatternBox_Loaded(object sender, RoutedEventArgs e)
    {
      if (sender is not TextBox box || box.DataContext is not HighlightRuleViewModel rule || !rule.FocusRequested)
        return;
      rule.FocusRequested = false;
      box.Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
      {
        box.BringIntoView();
        Keyboard.Focus(box);
      });
    }
  }
}
