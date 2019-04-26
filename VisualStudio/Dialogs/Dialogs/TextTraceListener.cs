using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  public class TextTraceListener : TraceListener, INotifyPropertyChanged
  {
    private readonly StringBuilder _builder;
    public bool IsDisposed;
    public TextTraceListener()
    {
      _builder = new StringBuilder();
    }

    public string Trace => _builder.ToString();

    public override void Write(string message)
    {
      if (string.IsNullOrWhiteSpace(message) || message.Equals("\n")) return;
      _builder.Append(message);
      OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
    }

    public override void WriteLine(string message)
    {
      if (string.IsNullOrWhiteSpace(message) || message.Equals("\n")) return;
      _builder.AppendLine(message);
      OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      PropertyChanged?.Invoke(this, e);
    }

    protected override void Dispose(bool disposing)
    {
      IsDisposed = true;
      base.Dispose(disposing);
    }
  }
}