using System;
using System.Collections.ObjectModel;
using System.Linq;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.Search
{
  public class SearchRequestViewModel : ViewModelBase
  {
    public Command SearchCommand { get; private set; }
    public Command AddTextCommand { get; private set; }
    public Command AddDateTimeCommand { get; private set; }
    public Command AndCommand { get; private set; }
    public Command OrCommand { get; private set; }
    public ObservableCollection<SearchRequestSelectableStatementViewModel> SearchRequestStatements { get; set; }

    public event Action<SearchRequest> SearchRequested;

    public SearchRequestViewModel()
    {
      this.SearchRequestStatements = new ObservableCollection<SearchRequestSelectableStatementViewModel>();
      this.SearchRequestStatements.CollectionChanged += SearchRequestStatements_CollectionChanged;
      this.AddNewTextStatement();
      this.InitCommands();
    }
    
    public void InitCommands()
    {
      this.SearchCommand = new Command(
        x => { this.ExecuteSearchRequest(); },
        x => true
        );
      this.AddTextCommand = new Command(
        x => { this.AddNewTextStatement(); },
        x => true
        );
      this.AddDateTimeCommand = new Command(
        x => { this.AddNewDateTimeStatement(); },
        x => true
        );
      this.AndCommand = new Command(
        x => { this.AddNewAndGroupStatement(); },
        x => true
        );
      this.OrCommand = new Command(
        x => { this.AddNewOrGroupStatement(); },
        x => true
        );
    }

    public void ExecuteSearchRequest()
    {
      var statements = this.SearchRequestStatements.Select(x => x.StatementViewModel.GetStatement());
      if (!statements.Any())
        return;
      var searchRequest = new SearchRequest();
      searchRequest.MultipleStatementsMode = this.GetMultipleStatementsMode();
      searchRequest.Statements.AddRange(statements);
      if (this.SearchRequested != null)
        this.SearchRequested.Invoke(searchRequest);
    }

    public void AddNewTextStatement()
    {
      var statement = new TextStatementViewModel(this);
      var selectableStatement = new SearchRequestSelectableStatementViewModel(this, statement);
      this.SearchRequestStatements.Add(selectableStatement);
    }

    public void AddNewDateTimeStatement()
    {
      var statement = new DateTimeStatementViewModel(this);
      var selectableStatement = new SearchRequestSelectableStatementViewModel(this, statement);
      this.SearchRequestStatements.Add(selectableStatement);
    }

    public void AddNewAndGroupStatement()
    {
      var checkedStatements = this.SearchRequestStatements.Where(x => x.Selected.GetValueOrDefault()).ToList();
      var statements = checkedStatements.Select(x => x.StatementViewModel);
      var andGroupStatement = new AndGroupStatementViewModel(statements);
      var selectableStatement = new SearchRequestSelectableStatementViewModel(this, andGroupStatement);
      this.SearchRequestStatements.Add(selectableStatement);
      foreach (var checkedStatement in checkedStatements)
        this.SearchRequestStatements.Remove(checkedStatement);
    }

    public void AddNewOrGroupStatement()
    {
      var checkedStatements = this.SearchRequestStatements.Where(x => x.Selected.GetValueOrDefault()).ToList();
      var statements = checkedStatements.Select(x => x.StatementViewModel);
      var andGroupStatement = new OrGroupStatementViewModel(statements);
      var selectableStatement = new SearchRequestSelectableStatementViewModel(this, andGroupStatement);
      this.SearchRequestStatements.Add(selectableStatement);
      foreach (var checkedStatement in checkedStatements)
        this.SearchRequestStatements.Remove(checkedStatement);
    }

    private void SearchRequestStatements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (this.SearchRequestStatements == null || !this.SearchRequestStatements.Any())
        return;
      if (this.SearchRequestStatements.Count == 1)
      {
        this.SearchRequestStatements.First().ShowCheckBox = false;
      }
      else
      {
        var statements = this.SearchRequestStatements.Where(x => !x.ShowCheckBox);
        foreach (var statement in statements)
          statement.ShowCheckBox = true;
      }
    }

    private MultipleStatementsMode GetMultipleStatementsMode()
    {
      return MultipleStatementsMode.Or;
    }
  }
}
