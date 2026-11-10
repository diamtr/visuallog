using System;
using System.Runtime.CompilerServices;

namespace VisualLog.Desktop.Settings
{
  /// <summary>Editable <see cref="HighlightRule"/> on the settings page.</summary>
  public class HighlightRuleViewModel : ViewModelBase
  {
    public const string SampleText = "Sample log line";

    /// <summary>Look of a line without any rule, used when the rule sets no style.</summary>
    private static readonly LineStyle NoStyle = new LineStyle(null, null, false, false, false);

    private readonly HighlightRule rule;

    /// <summary>Raised after any property of the rule was changed.</summary>
    public event Action<HighlightRuleViewModel> Changed;

    public bool IsEnabled
    {
      get => this.rule.IsEnabled;
      set { if (this.rule.IsEnabled != value) { this.rule.IsEnabled = value; this.OnRuleChanged(); } }
    }

    public string Pattern
    {
      get => this.rule.Pattern;
      set
      {
        value ??= string.Empty;
        if (this.rule.Pattern == value)
          return;
        this.rule.Pattern = value;
        this.OnRuleChanged();
      }
    }

    public bool IsRegex
    {
      get => this.rule.IsRegex;
      set { if (this.rule.IsRegex != value) { this.rule.IsRegex = value; this.OnRuleChanged(); } }
    }

    public bool MatchCase
    {
      get => this.rule.MatchCase;
      set { if (this.rule.MatchCase != value) { this.rule.MatchCase = value; this.OnRuleChanged(); } }
    }

    /// <summary>Text color "#RRGGBB" / "#AARRGGBB" or null.</summary>
    public string Foreground
    {
      get => this.rule.Foreground;
      set { value = Normalize(value); if (this.rule.Foreground != value) { this.rule.Foreground = value; this.OnRuleChanged(); } }
    }

    /// <summary>Background color "#RRGGBB" / "#AARRGGBB" or null.</summary>
    public string Background
    {
      get => this.rule.Background;
      set { value = Normalize(value); if (this.rule.Background != value) { this.rule.Background = value; this.OnRuleChanged(); } }
    }

    public bool Bold
    {
      get => this.rule.Bold;
      set { if (this.rule.Bold != value) { this.rule.Bold = value; this.OnRuleChanged(); } }
    }

    public bool Italic
    {
      get => this.rule.Italic;
      set { if (this.rule.Italic != value) { this.rule.Italic = value; this.OnRuleChanged(); } }
    }

    public bool Strikethrough
    {
      get => this.rule.Strikethrough;
      set { if (this.rule.Strikethrough != value) { this.rule.Strikethrough = value; this.OnRuleChanged(); } }
    }

    /// <summary>Invalid regular expression message, otherwise null.</summary>
    public string Error { get; private set; }

    public bool HasError => this.Error != null;

    /// <summary>The rule's look on its own. Never null: a rule without style gives the default look.</summary>
    public LineStyle Preview { get; private set; }

    /// <summary>True if the rule sets at least one style property.</summary>
    public bool HasStyle { get; private set; }

    /// <summary>Text of the preview chip: the pattern itself, or a sample for regexes and empty patterns.</summary>
    public string PreviewText => string.IsNullOrEmpty(this.Pattern) || this.IsRegex ? SampleText : this.Pattern;

    /// <summary>Set for a just added rule so the view can focus its pattern; the view resets it.</summary>
    public bool FocusRequested { get; set; }

    public HighlightRuleViewModel() : this(new HighlightRule())
    {
    }

    public HighlightRuleViewModel(HighlightRule rule)
    {
      this.rule = rule?.Clone() ?? new HighlightRule();
      this.rule.Pattern ??= string.Empty;
      this.Refresh();
    }

    /// <summary>A copy of the edited rule.</summary>
    public HighlightRule ToRule()
    {
      return this.rule.Clone();
    }

    private void OnRuleChanged([CallerMemberName] string prop = "")
    {
      this.OnPropertyChanged(prop);
      this.Refresh();
      this.OnPropertyChanged(nameof(this.Error));
      this.OnPropertyChanged(nameof(this.HasError));
      this.OnPropertyChanged(nameof(this.Preview));
      this.OnPropertyChanged(nameof(this.HasStyle));
      this.OnPropertyChanged(nameof(this.PreviewText));
      this.Changed?.Invoke(this);
    }

    private void Refresh()
    {
      this.Error = LineHighlighter.Validate(this.rule);
      var preview = LineHighlighter.Preview(this.rule);
      this.HasStyle = preview != null;
      this.Preview = preview ?? NoStyle;
    }

    private static string Normalize(string color)
    {
      return string.IsNullOrWhiteSpace(color) ? null : color.Trim();
    }
  }
}
