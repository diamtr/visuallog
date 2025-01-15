namespace VisualLog.Desktop.Search
{
  public class SearchRequestSelectableStatementViewModel : ViewModelBase
  {
    public bool ShowCheckBox {
      get { return this.showCheckBox; }
      set {
        this.showCheckBox = value;
        this.OnPropertyChanged();
      }
    }
    private bool showCheckBox;
    public bool? IsChecked
    {
      get { return this.isChecked; }
      set {
        this.isChecked = value;
        this.OnPropertyChanged();
      }
    }
    private bool? isChecked;
    public ISearchRequestStatementViewModel StatementViewModel {
      get { return this.statementViewModel; }
      private set {
        this.statementViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private ISearchRequestStatementViewModel statementViewModel;

    public SearchRequestSelectableStatementViewModel(ISearchRequestStatementViewModel statement) : this()
    {
      this.StatementViewModel = statement;
      this.IsChecked = false;
      this.ShowCheckBox = true;
    }
    public SearchRequestSelectableStatementViewModel() { }
  }
}
