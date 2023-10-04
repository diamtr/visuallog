using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// Interaction logic for MessageInlineView.xaml
  /// </summary>
  public partial class MessageInlineView : UserControl
  {
    public MessageInlineView()
    {
      InitializeComponent();
    }

    private void Message_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      var dataContext = (MessageInlineViewModel)this.DataContext;
      if (dataContext == null)
        return;
      dataContext.CopyEnabled = true;
    }

    private void Message_LostFocus(object sender, RoutedEventArgs e)
    {
      var dataContext = (MessageInlineViewModel)this.DataContext;
      if (dataContext == null)
        return;
      dataContext.CopyEnabled = false;
    }
  }
}
