namespace VisualLog.Desktop.Search
{
  public class SearchRequestSelectableStatementViewModel : ViewModelBase
  {
    public Command RemoveStatementCommand { get; private set; }
    public bool ShowCheckBox {
      get { return this.showCheckBox; }
      set {
        this.showCheckBox = value;
        this.OnPropertyChanged();
      }
    }
    private bool showCheckBox;
    public bool? Selected
    {
      get { return this.selected; }
      set {
        this.selected = value;
        this.OnPropertyChanged();
      }
    }
    private bool? selected;
    public ISearchRequestStatementViewModel StatementViewModel {
      get { return this.statementViewModel; }
      private set {
        this.statementViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private ISearchRequestStatementViewModel statementViewModel;

    private SearchRequestViewModel searchRequestViewModel;

    public SearchRequestSelectableStatementViewModel(SearchRequestViewModel searchRequestViewModel, ISearchRequestStatementViewModel statement) : this()
    {
      this.searchRequestViewModel = searchRequestViewModel;
      this.StatementViewModel = statement;
    }
    public SearchRequestSelectableStatementViewModel()
    {
      this.Selected = false;
      this.ShowCheckBox = true;
      this.InitCommands();
    }

    public void InitCommands()
    {
      this.RemoveStatementCommand = new Command(
        x => { this.searchRequestViewModel.SearchRequestStatements.Remove(this); },
        x => true
      );
    }


  }
}
