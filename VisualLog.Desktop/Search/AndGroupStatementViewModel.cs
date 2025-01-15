using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VisualLog.Core.Search;
using VisualLog.Core.Search.Statements;

namespace VisualLog.Desktop.Search
{
  public class AndGroupStatementViewModel : ViewModelBase, ISearchRequestStatementViewModel
  {
    public ObservableCollection<ISearchRequestStatementViewModel> Statements { get; set; }

    public AndGroupStatementViewModel(IEnumerable<ISearchRequestStatementViewModel> statements) : this()
    {
      this.Statements = new ObservableCollection<ISearchRequestStatementViewModel>();
      foreach (var statement in statements)
        this.Statements.Add(statement);
    }
    public AndGroupStatementViewModel() { }

    public ISearchRequestStatement GetStatement()
    {
      if (!this.Statements.Any())
        return null;

      var resultStatement = new AndGroupStatement();
      resultStatement.Statements.AddRange(this.Statements.Select(x => x.GetStatement()));

      return resultStatement;
    }
  }
}
