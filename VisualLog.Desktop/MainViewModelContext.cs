﻿namespace VisualLog.Desktop
{
  public class MainViewModelContext : ViewModelBase
  {
    public bool LogIsActive
    {
      get
      {
        return this.activeViewModel is LogViewModel;
      }
    }
    public bool FormatManagerIsActive
    {
      get
      {
        return this.activeViewModel is FormatManager.FormatManagerViewModel;
      }
    }
    
    private ViewModelBase activeViewModel;

    public void SetAsActive(ViewModelBase viewModel)
    {
      this.activeViewModel = viewModel;
      this.OnPropertyChanged(nameof(this.LogIsActive));
      this.OnPropertyChanged(nameof(this.FormatManagerIsActive));
    }
  }
}