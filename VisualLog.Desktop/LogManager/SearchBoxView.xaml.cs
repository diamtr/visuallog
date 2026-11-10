using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// Query box of the log toolbar.
  /// </summary>
  public partial class SearchBoxView : UserControl
  {
    /// <summary>Esc in the box: the log view takes the focus back.</summary>
    public event System.Action EscapeRequested;

    public SearchBoxView()
    {
      InitializeComponent();
      this.PreviewKeyDown += this.SearchBoxView_PreviewKeyDown;
    }

    public void FocusQuery()
    {
      this.QueryTextBox.Focus();
      Keyboard.Focus(this.QueryTextBox);
      this.QueryTextBox.SelectAll();
    }

    private void SearchBoxView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      var vm = this.DataContext as SearchViewModel;
      if (vm == null)
        return;

      // Alt+key arrives as Key.System
      var key = e.Key == Key.System ? e.SystemKey : e.Key;
      if (Keyboard.Modifiers == ModifierKeys.Alt && key == Key.R)
      {
        vm.UseRegex = !vm.UseRegex;
        e.Handled = true;
      }
      else if (Keyboard.Modifiers == ModifierKeys.Alt && key == Key.C)
      {
        vm.MatchCase = !vm.MatchCase;
        e.Handled = true;
      }
      else if (key == Key.Escape)
      {
        if (this.HistoryToggle.IsChecked == true)
          this.HistoryToggle.IsChecked = false;
        else
          this.EscapeRequested?.Invoke();
        e.Handled = true;
      }
      else if (key == Key.Down && Keyboard.Modifiers == ModifierKeys.Alt)
      {
        this.HistoryToggle.IsChecked = true;
        e.Handled = true;
      }
    }

    private void HistoryItem_Click(object sender, RoutedEventArgs e)
    {
      this.HistoryToggle.IsChecked = false;
      this.FocusQuery();
    }
  }
}
