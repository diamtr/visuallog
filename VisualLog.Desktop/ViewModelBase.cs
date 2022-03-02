using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace VisualLog.Desktop
{
  public class ViewModelBase : INotifyPropertyChanged
  {
    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;
    public virtual void OnPropertyChanged([CallerMemberName] string prop = "")
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

    public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression == null)
      {
        return;
      }

      var handler = PropertyChanged;

      if (handler != null)
      {
        var body = propertyExpression.Body as MemberExpression;
        if (body != null)
          handler(this, new PropertyChangedEventArgs(body.Member.Name));
      }
    }

    #endregion
  }
}
