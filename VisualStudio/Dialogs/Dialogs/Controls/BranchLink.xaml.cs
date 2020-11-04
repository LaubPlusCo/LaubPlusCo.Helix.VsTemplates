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
  public partial class BranchLink : UserControl
  {
    protected readonly HelixTemplatesBranch Branch;

    public BranchLink(HelixTemplatesBranch branch)
    {
      InitializeComponent();
      Branch = branch;
      BranchName.Text = Branch.DisplayName;
      this.SetVisualStudioThemeStyles();
    }

    private void ViewBranchClicked(object sender, RoutedEventArgs e)
    {
      throw new System.NotImplementedException();
    }
  }
}