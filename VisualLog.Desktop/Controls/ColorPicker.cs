using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.Controls
{
  /// <summary>
  /// Compact color picker: a swatch opening a card with preset colors, "None" and a hex box.
  /// <see cref="SelectedColor"/> is "#RRGGBB" / "#AARRGGBB" or null (no color).
  /// The look is defined by the implicit ColorPicker style in Styles.xaml.
  /// </summary>
  public class ColorPicker : Control
  {
    /// <summary>A preset color of the drop-down.</summary>
    public sealed class Swatch
    {
      public string Hex { get; }
      public Brush Brush { get; }

      public Swatch(string hex)
      {
        this.Hex = hex;
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        this.Brush = brush;
      }
    }

    /// <summary>Soft tints (backgrounds), mid tones and strong colors (text), one hue per column.</summary>
    public static IReadOnlyList<Swatch> Presets { get; } = new[]
    {
      "#FDE7E9", "#FDF0E6", "#FFF4CE", "#DFF6DD", "#DEECF9", "#EFE6F7", "#F0F0F0",
      "#F1707B", "#F7A35C", "#FFD335", "#6CCB5F", "#62ABF5", "#B293D6", "#BDBDBD",
      "#C50F1F", "#CA5010", "#986F0B", "#107C10", "#0F6CBD", "#5C2E91", "#616161",
    }.Select(x => new Swatch(x)).ToArray();

    public static readonly DependencyProperty SelectedColorProperty =
      DependencyProperty.Register(nameof(SelectedColor), typeof(string), typeof(ColorPicker),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register(nameof(Label), typeof(string), typeof(ColorPicker), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsDropDownOpenProperty =
      DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(ColorPicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty HexTextProperty =
      DependencyProperty.Register(nameof(HexText), typeof(string), typeof(ColorPicker),
        new PropertyMetadata(string.Empty, OnHexTextChanged));

    private static readonly DependencyPropertyKey SelectedBrushPropertyKey =
      DependencyProperty.RegisterReadOnly(nameof(SelectedBrush), typeof(Brush), typeof(ColorPicker), new PropertyMetadata(null));
    public static readonly DependencyProperty SelectedBrushProperty = SelectedBrushPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey HasColorPropertyKey =
      DependencyProperty.RegisterReadOnly(nameof(HasColor), typeof(bool), typeof(ColorPicker), new PropertyMetadata(false));
    public static readonly DependencyProperty HasColorProperty = HasColorPropertyKey.DependencyProperty;

    // Set while the hex box is the source of the change, so typing is not rewritten under the caret
    private bool isApplyingHex;

    public ColorPicker()
    {
      this.SelectColorCommand = new Command(x =>
      {
        this.SelectedColor = x as string;
        this.IsDropDownOpen = false;
      });
    }

    /// <summary>"#RRGGBB" / "#AARRGGBB" or null.</summary>
    public string SelectedColor
    {
      get => (string)this.GetValue(SelectedColorProperty);
      set => this.SetValue(SelectedColorProperty, value);
    }

    /// <summary>Short caption next to the swatch and title of the drop-down.</summary>
    public string Label
    {
      get => (string)this.GetValue(LabelProperty);
      set => this.SetValue(LabelProperty, value);
    }

    public bool IsDropDownOpen
    {
      get => (bool)this.GetValue(IsDropDownOpenProperty);
      set => this.SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>Text of the hex box; a valid color typed here becomes <see cref="SelectedColor"/>.</summary>
    public string HexText
    {
      get => (string)this.GetValue(HexTextProperty);
      set => this.SetValue(HexTextProperty, value);
    }

    /// <summary>Brush of <see cref="SelectedColor"/>, null if there is no (valid) color.</summary>
    public Brush SelectedBrush => (Brush)this.GetValue(SelectedBrushProperty);

    public bool HasColor => (bool)this.GetValue(HasColorProperty);

    /// <summary>Selects the color passed as parameter (null for none) and closes the drop-down.</summary>
    public ICommand SelectColorCommand { get; }

    public IReadOnlyList<Swatch> Colors => Presets;

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var picker = (ColorPicker)d;
      var color = LineStyle.ParseColor((string)e.NewValue);
      Brush brush = null;
      if (color.HasValue)
      {
        brush = new SolidColorBrush(color.Value);
        brush.Freeze();
      }
      picker.SetValue(SelectedBrushPropertyKey, brush);
      picker.SetValue(HasColorPropertyKey, brush != null);
      if (!picker.isApplyingHex)
        picker.HexText = (string)e.NewValue ?? string.Empty;
    }

    private static void OnHexTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var picker = (ColorPicker)d;
      var text = ((string)e.NewValue ?? string.Empty).Trim();
      string newColor;
      if (text.Length == 0)
        newColor = null;
      else if (LineStyle.ParseColor(text).HasValue)
        newColor = text;
      else
        return; // incomplete input, keep the current color

      if (newColor == picker.SelectedColor)
        return;
      picker.isApplyingHex = true;
      try
      {
        picker.SelectedColor = newColor;
      }
      finally
      {
        picker.isApplyingHex = false;
      }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      if (this.IsDropDownOpen && (e.Key == Key.Escape || e.Key == Key.Enter))
      {
        this.IsDropDownOpen = false;
        e.Handled = true;
      }
    }
  }
}
