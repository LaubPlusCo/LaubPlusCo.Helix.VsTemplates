using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;
using Microsoft.VisualStudio.PlatformUI;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for SettingsDialog.xaml
  /// </summary>
  public partial class TraceWindow : DialogWindow
  {
    protected readonly TextTraceListener TraceListener;

    public TraceWindow(TextTraceListener traceListener)
    {
      TraceListener = traceListener;
      WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(TraceListener, "PropertyChanged",
        TraceListenerOnPropertyChanged);
      Trace.Listeners.Add(TraceListener);
      InitializeComponent();
      this.SetVisualStudioThemeStyles();
      SetTraceText(TraceListener.Trace);
    }

    private void TraceListenerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName != "Trace")
        return;
      if (Dispatcher.CheckAccess())
        SetTraceText(TraceListener.Trace);
    }

    private void SetTraceText(string text)
    {
      TraceTextBox.Text = text;
      TraceTextBox.ScrollToEnd();
    }

    private void CloseDialog(object sender, RoutedEventArgs e)
    {
      WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.RemoveHandler(TraceListener, "PropertyChanged",
        TraceListenerOnPropertyChanged);
      Close();
    }
  }
}