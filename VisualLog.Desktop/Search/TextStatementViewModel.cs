using System.Collections.Generic;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.Search
{
  public class TextStatementViewModel : ViewModelBase, ISearchRequestStatementViewModel
  {
    public string Text
    {
      get { return this.text; }
      set
      {
        this.text = value;
        this.OnPropertyChanged();
      }
    }
    private string text;

    public SearchRequestViewModel SearchRequestViewModel
    {
      get { return this.searchRequestViewModel; }
      set
      {
        this.searchRequestViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private SearchRequestViewModel searchRequestViewModel;

    public TextStatementViewModel(SearchRequestViewModel searchRequestViewModel) : this()
    {
      this.SearchRequestViewModel = searchRequestViewModel;
    }

    public TextStatementViewModel() { }

    public ISearchRequestStatement GetStatement()
    {
      return new TextStatement() { Text = this.Text };
    }
  }
}
