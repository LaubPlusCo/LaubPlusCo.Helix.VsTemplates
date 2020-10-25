using System.Windows;
using System.Windows.Navigation;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.PlatformUI;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for VersionMessageDialog.xaml
  /// </summary>
  public partial class VersionMessageDialog : DialogWindow
  {
    public VersionMessageDialog()
    {
      InitializeComponent();
      Headline.Text = $"v{AppScopeSettings.Current.InstalledVersion} Release notes";
      this.SetVisualStudioThemeStyles();
    }

    private void Close_Clicked(object sender, RoutedEventArgs e)
    {
      AppScopeSettings.Current.SetVersionNoticeShown();
      Close();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
    }
  }
}