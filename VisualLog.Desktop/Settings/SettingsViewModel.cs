using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace VisualLog.Desktop.Settings
{
  /// <summary>
  /// Application settings page. For now only line highlighting rules.
  /// Every change is applied to <see cref="HighlightingService"/> and saved;
  /// rule edits (typing a pattern) are debounced, add/remove/move are applied at once.
  /// </summary>
  public class SettingsViewModel : ViewModelBase
  {
    public static readonly TimeSpan ApplyDelay = TimeSpan.FromMilliseconds(300);

    /// <summary>Background of a new rule, so it is visible as soon as a pattern is typed.</summary>
    public const string NewRuleBackground = "#FFF4CE";

    private readonly HighlightingService highlighting;
    private DispatcherTimer applyTimer;

    /// <summary>Settings file. Tests point it to a temp file.</summary>
    public string StoragePath { get; set; }

    /// <summary>Rules in priority order: the topmost wins for colors.</summary>
    public ObservableCollection<HighlightRuleViewModel> HighlightRules { get; } = new ObservableCollection<HighlightRuleViewModel>();

    public bool HasRules => this.HighlightRules.Count > 0;

    /// <summary>True while a debounced edit has not been applied yet.</summary>
    public bool HasPendingChanges { get; private set; }

    public Command AddRuleCommand { get; }
    public Command RemoveRuleCommand { get; }
    public Command MoveRuleUpCommand { get; }
    public Command MoveRuleDownCommand { get; }

    public SettingsViewModel(HighlightingService highlighting, string storagePath = null)
    {
      this.highlighting = highlighting ?? throw new ArgumentNullException(nameof(highlighting));
      this.StoragePath = storagePath ?? AppSettings.DefaultPath;
      this.HighlightRules.CollectionChanged += (s, e) => this.OnPropertyChanged(nameof(this.HasRules));

      this.AddRuleCommand = new Command(x => this.AddRule());
      this.RemoveRuleCommand = new Command(
        x => this.RemoveRule(x as HighlightRuleViewModel),
        x => x is HighlightRuleViewModel);
      this.MoveRuleUpCommand = new Command(
        x => this.MoveRule(x as HighlightRuleViewModel, -1),
        x => x is HighlightRuleViewModel rule && this.HighlightRules.IndexOf(rule) > 0);
      this.MoveRuleDownCommand = new Command(
        x => this.MoveRule(x as HighlightRuleViewModel, 1),
        x => x is HighlightRuleViewModel rule && this.HighlightRules.IndexOf(rule) is int i &&
             i >= 0 && i < this.HighlightRules.Count - 1);
    }

    /// <summary>Reads the settings file and applies it. Doesn't write anything.</summary>
    public void Load()
    {
      this.StopTimer();
      var settings = AppSettings.Load(this.StoragePath);
      foreach (var rule in this.HighlightRules)
        rule.Changed -= this.OnRuleChanged;
      this.HighlightRules.Clear();
      foreach (var rule in settings.HighlightRules)
        this.Attach(new HighlightRuleViewModel(rule), this.HighlightRules.Count);
      this.highlighting.Update(this.ToRules());
    }

    public HighlightRuleViewModel AddRule()
    {
      var rule = new HighlightRuleViewModel(new HighlightRule { Background = NewRuleBackground });
      rule.FocusRequested = true;
      this.Attach(rule, this.HighlightRules.Count);
      this.ApplyNow();
      return rule;
    }

    public void RemoveRule(HighlightRuleViewModel rule)
    {
      if (rule == null || !this.HighlightRules.Remove(rule))
        return;
      rule.Changed -= this.OnRuleChanged;
      this.ApplyNow();
    }

    /// <summary>Moves a rule by <paramref name="offset"/> positions (negative is up).</summary>
    public void MoveRule(HighlightRuleViewModel rule, int offset)
    {
      var index = this.HighlightRules.IndexOf(rule);
      if (index < 0)
        return;
      var newIndex = Math.Clamp(index + offset, 0, this.HighlightRules.Count - 1);
      if (newIndex == index)
        return;
      this.HighlightRules.Move(index, newIndex);
      this.ApplyNow();
    }

    /// <summary>Rebuilds the highlighting and saves the settings right away.</summary>
    public void ApplyNow()
    {
      this.StopTimer();
      var rules = this.ToRules();
      this.highlighting.Update(rules);
      new AppSettings { HighlightRules = rules }.Save(this.StoragePath);
    }

    /// <summary>Applies a debounced edit if there is one, e.g. when the window closes.</summary>
    public void ApplyPending()
    {
      if (this.HasPendingChanges)
        this.ApplyNow();
    }

    public List<HighlightRule> ToRules()
    {
      return this.HighlightRules.Select(x => x.ToRule()).ToList();
    }

    private void Attach(HighlightRuleViewModel rule, int index)
    {
      rule.Changed += this.OnRuleChanged;
      this.HighlightRules.Insert(index, rule);
    }

    private void OnRuleChanged(HighlightRuleViewModel rule)
    {
      // Restart the delay on every keystroke, apply once the user pauses
      if (this.applyTimer == null)
      {
        this.applyTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = ApplyDelay };
        this.applyTimer.Tick += (s, e) => this.ApplyNow();
      }
      this.applyTimer.Stop();
      this.applyTimer.Start();
      this.HasPendingChanges = true;
    }

    private void StopTimer()
    {
      this.applyTimer?.Stop();
      this.HasPendingChanges = false;
    }
  }
}
