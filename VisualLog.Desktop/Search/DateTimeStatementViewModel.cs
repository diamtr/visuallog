using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.Search
{
  public class DateTimeStatementViewModel : ViewModelBase, ISearchRequestStatementViewModel
  {
    public string DateTimeString {
      get { return this.dateTimeString; }
      set {
        this.dateTimeString = value;
        this.OnPropertyChanged();
      }
    }
    private string dateTimeString;
    public ObservableCollection<string> DateTimeStatementConditions { get; set; }
    public string SelectedDateTimeStatementCondition {
      get { return this.selectedDateTimeStatementCondition; }
      set {
        this.selectedDateTimeStatementCondition = value;
        this.OnPropertyChanged();
      }
    }
    private string selectedDateTimeStatementCondition;

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

    public DateTimeStatementViewModel(SearchRequestViewModel searchRequestViewModel) : this()
    {
      this.SearchRequestViewModel = searchRequestViewModel;
    }

    public DateTimeStatementViewModel() {
      this.DateTimeStatementConditions = new ObservableCollection<string>();
      this.InitDateTimeStatementConditions();
    }

    public ISearchRequestStatement GetStatement()
    {
      DateTimeStatementCondition condition;
      var conditionParsed = Enum.TryParse<DateTimeStatementCondition>(this.SelectedDateTimeStatementCondition, out condition);
      DateTime dateTime;
      var dateTimeParsed = DateTime.TryParse(this.SelectedDateTimeStatementCondition, out dateTime);
      if (!dateTimeParsed || !conditionParsed)
        return null;

      return new DateTimeStatement() { DateTime = dateTime, Condition = condition };
    }

    private void InitDateTimeStatementConditions()
    {
      var values = Enum.GetValues(typeof(DateTimeStatementCondition));
      foreach (var value in values)
        this.DateTimeStatementConditions.Add(Enum.GetName(typeof(DateTimeStatementCondition), value));
    }
  }
}
