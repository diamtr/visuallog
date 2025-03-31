using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VisualLog.Core.Search.Statements;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.Search
{
  internal class OrGroupStatementViewModel : ViewModelBase, ISearchRequestStatementViewModel
  {
    public ObservableCollection<ISearchRequestStatementViewModel> Statements { get; set; }

    public OrGroupStatementViewModel(IEnumerable<ISearchRequestStatementViewModel> statements) : this()
    {
      this.Statements = new ObservableCollection<ISearchRequestStatementViewModel>();
      foreach (var statement in statements)
        this.Statements.Add(statement);
    }
    public OrGroupStatementViewModel() { }

    public ISearchRequestStatement GetStatement()
    {
      if (!this.Statements.Any())
        return null;

      var resultStatement = new OrGroupStatement();
      resultStatement.Statements.AddRange(this.Statements.Select(x => x.GetStatement()));

      return resultStatement;
    }
  }
}
