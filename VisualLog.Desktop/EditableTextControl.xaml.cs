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
  /// Interaction logic for EditableTextControl.xaml
  /// </summary>
  public partial class EditableTextControl : UserControl
  {

    /// <summary>
    /// Текст.
    /// </summary>
    public string Text
    {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
          "Text",
          typeof(string),
          typeof(EditableTextControl),
          new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(TextChangedCallback)));

    public static void TextChangedCallback(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
    {
      var control = (EditableTextControl)depObj;
      var textBox = control.TextBox;
      textBox.Text = args.NewValue.ToString();
      var textBlock = control.TextBlock;
      textBlock.Text = args.NewValue.ToString();
    }

    public EditableTextControl()
    {
      InitializeComponent();
    }

    private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.TextBlock.Visibility = Visibility.Hidden;
      this.TextBox.Focus();
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      this.TextBlock.Visibility = Visibility.Visible;
    }
  }
}
