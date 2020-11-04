using System.IO;
using System.Windows;
using System.Windows.Controls;
using LaubPlusCo.Foundation.Helix.TemplateRepository.Data;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs.Controls
{
  /// <summary>
  ///   Interaction logic for TokenFolderInput.xaml
  /// </summary>
  public partial class BranchType : UserControl
  {
    protected readonly HelixTemplatesBranch[] Branches;
    protected bool IsOpen = false;

    public BranchType(string typeName, HelixTemplatesBranch[] branches)
    {
      InitializeComponent();
      Branches = branches;
      BranchTypeName.Text = typeName;
      BranchTypeName.MouseLeftButtonDown += BranchTypeNameClicked;
      SetBranchLinks();
      BranchLinksPanel.Visibility = Visibility.Collapsed;
      this.SetVisualStudioThemeStyles();
    }

    private void SetBranchLinks()
    {
      BranchLinksPanel.Children.Clear();
      foreach (var helixTemplatesBranch in Branches)
      {
        BranchLinksPanel.Children.Add(new BranchLink(helixTemplatesBranch));
      }
    }

    private void BranchTypeNameClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      if (BranchLinksPanel.Visibility != Visibility.Visible)
      {
        BranchLinksPanel.Visibility = Visibility.Visible;
        return;
      }
      BranchLinksPanel.Visibility = Visibility.Collapsed;
    }
  }
}