using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for ExploreDialog.xaml
  /// </summary>
  public partial class ExploreDialog : DialogWindow
  {
    public bool ReloadOnClose = false;
    protected readonly string SolutionRootDirectory;
    protected readonly SolutionScopeSettings SolutionScopeSettings;

    public ExploreDialog() : this(string.Empty, false)
    {
        var slideIn = new DoubleAnimation(0, ActualWidth, TimeSpan.FromSeconds(1), FillBehavior.HoldEnd);
    }

    public ExploreDialog(string solutionRootDirectory, bool isSolutionCreation)
    {
      InitializeComponent();
      this.SetVisualStudioThemeStyles();
      ExploreHeadline.MouseLeftButtonDown += (sender, args) => { DragMove(); };
    }


    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      Process.Start(e.Uri.AbsoluteUri);
    }

    private void CloseButtonClicked(object sender, RoutedEventArgs e)
    {
      DialogResult = ReloadOnClose;
      Close();
    }
  }
}