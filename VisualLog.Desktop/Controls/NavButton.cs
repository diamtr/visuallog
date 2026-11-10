using System.Windows;
using System.Windows.Controls;

namespace VisualLog.Desktop.Controls
{
  /// <summary>
  /// Item of the left navigation rail: icon glyph + caption, selected state and optional counter badge.
  /// The look is defined by the "NavButton" style in Styles.xaml.
  /// </summary>
  public class NavButton : Button
  {
    public static readonly DependencyProperty GlyphProperty =
      DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(NavButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register(nameof(Label), typeof(string), typeof(NavButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsSelectedProperty =
      DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(NavButton), new PropertyMetadata(false));

    public static readonly DependencyProperty BadgeCountProperty =
      DependencyProperty.Register(nameof(BadgeCount), typeof(int), typeof(NavButton), new PropertyMetadata(0));

    public string Glyph
    {
      get => (string)this.GetValue(GlyphProperty);
      set => this.SetValue(GlyphProperty, value);
    }

    public string Label
    {
      get => (string)this.GetValue(LabelProperty);
      set => this.SetValue(LabelProperty, value);
    }

    public bool IsSelected
    {
      get => (bool)this.GetValue(IsSelectedProperty);
      set => this.SetValue(IsSelectedProperty, value);
    }

    public int BadgeCount
    {
      get => (int)this.GetValue(BadgeCountProperty);
      set => this.SetValue(BadgeCountProperty, value);
    }
  }
}
