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

namespace VisualLog.Desktop
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainView : Window
  {
    public MainView()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var mvm = this.DataContext as MainViewModel;
      if (mvm == null)
        return;
      mvm.OnWindowLoaded();
    }

    private void Window_PreviewDragOver(object sender, DragEventArgs e)
    {
      // Only files are intercepted, so text drag & drop inside text boxes keeps working.
      if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        return;
      e.Effects = DragDropEffects.Copy;
      e.Handled = true;
    }

    private void Window_PreviewDrop(object sender, DragEventArgs e)
    {
      if (this.DataContext is not MainViewModel mvm ||
          e.Data.GetData(DataFormats.FileDrop) is not string[] paths)
        return;
      e.Handled = true;
      mvm.OpenAndShow(paths);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      var mvm = this.DataContext as MainViewModel;
      if (mvm == null)
        return;
      mvm.OnWindowClosing();
    }
  }
}
