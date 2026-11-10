using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class SettingsViewModelFixture
  {
    private string tempDir;
    private string storagePath;
    private HighlightingService highlighting;

    [SetUp]
    public void SetUp()
    {
      this.tempDir = Path.Combine(Path.GetTempPath(), "VisualLogTests_" + Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(this.tempDir);
      this.storagePath = Path.Combine(this.tempDir, AppSettings.FileName);
      this.highlighting = new HighlightingService();
    }

    [TearDown]
    public void TearDown()
    {
      try { Directory.Delete(this.tempDir, true); } catch { }
    }

    private SettingsViewModel Create()
    {
      var svm = new SettingsViewModel(this.highlighting, this.storagePath);
      svm.Load();
      return svm;
    }

    private static Color ColorOf(Brush brush) => ((SolidColorBrush)brush).Color;

    [Test]
    public void DefaultStoragePath()
    {
      var svm = new SettingsViewModel(this.highlighting);
      Assert.That(svm.StoragePath, Is.EqualTo(AppSettings.DefaultPath));
    }

    [Test]
    public void ShowLineNumbersIsSavedAndShared()
    {
      var display = new DisplaySettings();
      var svm = new SettingsViewModel(this.highlighting, this.storagePath, display);
      svm.Load();
      Assert.That(svm.ShowLineNumbers, Is.True);

      svm.AddRule().Pattern = "ERROR";
      svm.ShowLineNumbers = false;
      Assert.That(display.ShowLineNumbers, Is.False);
      var saved = AppSettings.Load(this.storagePath);
      Assert.That(saved.ShowLineNumbers, Is.False);
      Assert.That(saved.HighlightRules.Single().Pattern, Is.EqualTo("ERROR"));

      var reloaded = new SettingsViewModel(new HighlightingService(), this.storagePath);
      reloaded.Load();
      Assert.That(reloaded.ShowLineNumbers, Is.False);
      Assert.That(reloaded.Display.ShowLineNumbers, Is.False);
    }

    [Test]
    public void LoadMissingFile()
    {
      var svm = Create();

      Assert.That(svm.HighlightRules, Is.Empty);
      Assert.That(svm.HasRules, Is.False);
      Assert.That(this.highlighting.Current.IsEmpty, Is.True);
      // Loading never writes
      Assert.That(File.Exists(this.storagePath), Is.False);
    }

    [Test]
    public void LoadBrokenFile()
    {
      File.WriteAllText(this.storagePath, "{ this is not json");
      var svm = Create();

      Assert.That(svm.HighlightRules, Is.Empty);
      Assert.That(File.ReadAllText(this.storagePath), Is.EqualTo("{ this is not json"));
    }

    [Test]
    public void AddRule()
    {
      var svm = Create();
      var changed = 0;
      this.highlighting.Changed += () => changed++;

      svm.AddRuleCommand.Execute(null);

      Assert.That(svm.HighlightRules.Count, Is.EqualTo(1));
      Assert.That(svm.HasRules, Is.True);
      Assert.That(svm.HighlightRules[0].IsEnabled, Is.True);
      Assert.That(svm.HighlightRules[0].Pattern, Is.Empty);
      Assert.That(changed, Is.EqualTo(1));
      Assert.That(File.Exists(this.storagePath), Is.True);
    }

    [Test]
    public void RemoveRule()
    {
      var svm = Create();
      var first = svm.AddRule();
      var second = svm.AddRule();

      Assert.That(svm.RemoveRuleCommand.CanExecute(first), Is.True);
      svm.RemoveRuleCommand.Execute(first);

      Assert.That(svm.HighlightRules.Single(), Is.SameAs(second));
      Assert.That(AppSettings.Load(this.storagePath).HighlightRules.Count, Is.EqualTo(1));

      // Edits of a removed rule are not tracked anymore
      first.Pattern = "x";
      Assert.That(svm.HasPendingChanges, Is.False);
    }

    [Test]
    public void MoveRules()
    {
      var svm = Create();
      var a = svm.AddRule();
      a.Pattern = "a";
      var b = svm.AddRule();
      b.Pattern = "b";
      var c = svm.AddRule();
      c.Pattern = "c";

      Assert.That(svm.MoveRuleUpCommand.CanExecute(a), Is.False);
      Assert.That(svm.MoveRuleDownCommand.CanExecute(c), Is.False);
      Assert.That(svm.MoveRuleUpCommand.CanExecute(b), Is.True);
      Assert.That(svm.MoveRuleDownCommand.CanExecute(b), Is.True);

      svm.MoveRuleUpCommand.Execute(c);
      Assert.That(svm.HighlightRules.Select(x => x.Pattern), Is.EqualTo(new[] { "a", "c", "b" }));

      svm.MoveRuleDownCommand.Execute(a);
      Assert.That(svm.HighlightRules.Select(x => x.Pattern), Is.EqualTo(new[] { "c", "a", "b" }));

      // Out of range moves are ignored
      svm.MoveRule(c, -5);
      Assert.That(svm.HighlightRules.Select(x => x.Pattern), Is.EqualTo(new[] { "c", "a", "b" }));

      // Moves are applied and saved immediately
      Assert.That(svm.HasPendingChanges, Is.False);
      Assert.That(AppSettings.Load(this.storagePath).HighlightRules.Select(x => x.Pattern),
                  Is.EqualTo(new[] { "c", "a", "b" }));
    }

    [Test]
    public void EditIsDebouncedUntilApplyNow()
    {
      var svm = Create();
      var rule = svm.AddRule();

      rule.Pattern = "ERROR";
      rule.Foreground = "#C50F1F";
      rule.Bold = true;

      // Not applied yet: waits for the debounce timer
      Assert.That(svm.HasPendingChanges, Is.True);
      Assert.That(this.highlighting.Current.Match("an ERROR line"), Is.Null);

      svm.ApplyNow();

      Assert.That(svm.HasPendingChanges, Is.False);
      var style = this.highlighting.Current.Match("an ERROR line");
      Assert.That(style, Is.Not.Null);
      Assert.That(ColorOf(style.Foreground), Is.EqualTo(Color.FromRgb(0xC5, 0x0F, 0x1F)));
      Assert.That(style.IsBold, Is.True);
      Assert.That(this.highlighting.Current.Match("an info line"), Is.Null);
    }

    [Test]
    public void TopmostRuleWins()
    {
      var svm = Create();
      var error = svm.AddRule();
      error.Pattern = "error";
      error.Background = "#FDE7E9";
      var any = svm.AddRule();
      any.Pattern = "line";
      any.Background = "#DEECF9";
      any.Italic = true;
      svm.ApplyNow();

      var style = this.highlighting.Current.Match("error line");
      Assert.That(ColorOf(style.Background), Is.EqualTo(Color.FromRgb(0xFD, 0xE7, 0xE9)));
      Assert.That(style.IsItalic, Is.True);

      svm.MoveRule(any, -1);
      style = this.highlighting.Current.Match("error line");
      Assert.That(ColorOf(style.Background), Is.EqualTo(Color.FromRgb(0xDE, 0xEC, 0xF9)));
    }

    [Test]
    public void DisabledRuleIsIgnored()
    {
      var svm = Create();
      var rule = svm.AddRule();
      rule.Pattern = "ERROR";
      rule.IsEnabled = false;
      svm.ApplyNow();

      Assert.That(this.highlighting.Current.Match("ERROR"), Is.Null);
    }

    [Test]
    public void InvalidRegex()
    {
      var svm = Create();
      var bad = svm.AddRule();
      bad.IsRegex = true;
      bad.Pattern = "ERR(";
      var good = svm.AddRule();
      good.IsRegex = true;
      good.Pattern = "WARN\\d+";
      svm.ApplyNow();

      Assert.That(bad.Error, Is.Not.Null);
      Assert.That(bad.HasError, Is.True);
      Assert.That(good.Error, Is.Null);
      Assert.That(this.highlighting.Current.Match("ERR( here"), Is.Null);
      Assert.That(this.highlighting.Current.Match("WARN42 here"), Is.Not.Null);

      bad.Pattern = "ERR\\(";
      Assert.That(bad.Error, Is.Null);
    }

    [Test]
    public void SaveAndReload()
    {
      var svm = Create();
      var rule = svm.AddRule();
      rule.Pattern = "timeout";
      rule.MatchCase = true;
      rule.IsRegex = false;
      rule.Foreground = "#0F6CBD";
      rule.Background = null;
      rule.Italic = true;
      rule.Strikethrough = true;
      var second = svm.AddRule();
      second.Pattern = "^\\d+$";
      second.IsRegex = true;
      second.IsEnabled = false;
      svm.ApplyNow();

      var otherService = new HighlightingService();
      var reloaded = new SettingsViewModel(otherService, this.storagePath);
      reloaded.Load();

      Assert.That(reloaded.HighlightRules.Count, Is.EqualTo(2));
      var r = reloaded.HighlightRules[0];
      Assert.That(r.Pattern, Is.EqualTo("timeout"));
      Assert.That(r.MatchCase, Is.True);
      Assert.That(r.IsRegex, Is.False);
      Assert.That(r.Foreground, Is.EqualTo("#0F6CBD"));
      Assert.That(r.Background, Is.Null);
      Assert.That(r.Bold, Is.False);
      Assert.That(r.Italic, Is.True);
      Assert.That(r.Strikethrough, Is.True);
      var s = reloaded.HighlightRules[1];
      Assert.That(s.Pattern, Is.EqualTo("^\\d+$"));
      Assert.That(s.IsRegex, Is.True);
      Assert.That(s.IsEnabled, Is.False);

      // Load applies the rules to the service
      Assert.That(otherService.Current.Match("a timeout"), Is.Not.Null);
      Assert.That(otherService.Current.Match("a TIMEOUT"), Is.Null);
    }

    [Test]
    public void ApplyPending()
    {
      var svm = Create();
      var rule = svm.AddRule();
      rule.Pattern = "ERROR";

      svm.ApplyPending();
      Assert.That(this.highlighting.Current.Match("ERROR"), Is.Not.Null);
      Assert.That(AppSettings.Load(this.storagePath).HighlightRules.Single().Pattern, Is.EqualTo("ERROR"));
    }

    [Test]
    public void RuleViewModelPreview()
    {
      var rule = new HighlightRuleViewModel();
      var changed = 0;
      rule.Changed += x => changed++;

      // No style: default look, never null
      Assert.That(rule.HasStyle, Is.False);
      Assert.That(rule.Preview, Is.Not.Null);
      Assert.That(rule.Preview.Foreground, Is.SameAs(LineStyle.DefaultForeground));
      Assert.That(rule.PreviewText, Is.EqualTo(HighlightRuleViewModel.SampleText));

      rule.Pattern = "ERROR";
      rule.Background = "#FFF4CE";
      rule.Bold = true;
      Assert.That(changed, Is.EqualTo(3));
      Assert.That(rule.HasStyle, Is.True);
      Assert.That(rule.PreviewText, Is.EqualTo("ERROR"));
      Assert.That(ColorOf(rule.Preview.Background), Is.EqualTo(Color.FromRgb(0xFF, 0xF4, 0xCE)));
      Assert.That(rule.Preview.FontWeight, Is.EqualTo(FontWeights.Bold));

      // Same value: no change
      rule.Bold = true;
      Assert.That(changed, Is.EqualTo(3));

      // Regex patterns are previewed on sample text
      rule.IsRegex = true;
      Assert.That(rule.PreviewText, Is.EqualTo(HighlightRuleViewModel.SampleText));

      // Empty color means no color
      rule.Background = "  ";
      Assert.That(rule.Background, Is.Null);

      // ToRule gives a copy
      var copy = rule.ToRule();
      copy.Pattern = "other";
      Assert.That(rule.Pattern, Is.EqualTo("ERROR"));
    }

    [Test]
    public void RuleViewModelNotifiesErrorAndPreview()
    {
      var rule = new HighlightRuleViewModel(new HighlightRule { IsRegex = true });
      var props = new System.Collections.Generic.List<string>();
      rule.PropertyChanged += (s, e) => props.Add(e.PropertyName);

      rule.Pattern = "(";

      Assert.That(props, Does.Contain(nameof(HighlightRuleViewModel.Pattern)));
      Assert.That(props, Does.Contain(nameof(HighlightRuleViewModel.Error)));
      Assert.That(props, Does.Contain(nameof(HighlightRuleViewModel.Preview)));
      Assert.That(rule.HasError, Is.True);
    }
  }
}
