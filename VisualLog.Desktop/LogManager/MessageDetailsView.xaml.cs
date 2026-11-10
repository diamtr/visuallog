using System.Windows.Controls;
using System.Windows.Input;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>Details of one line of the log: <see cref="MessageDetailsViewModel"/>.</summary>
  public partial class MessageDetailsView : UserControl
  {
    public MessageDetailsView()
    {
      InitializeComponent();
    }

    /// <summary>Ctrl+C in the tree: the selected part of a value if any, otherwise the whole value of the selected node.</summary>
    private void FieldsTree_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.C || Keyboard.Modifiers != ModifierKeys.Control)
        return;
      if (Keyboard.FocusedElement is TextBox box && box.SelectionLength > 0)
        return;
      if ((sender as TreeView)?.SelectedItem is JsonNodeViewModel node)
      {
        node.CopyValueCommand.Execute(null);
        e.Handled = true;
      }
    }
  }
}
