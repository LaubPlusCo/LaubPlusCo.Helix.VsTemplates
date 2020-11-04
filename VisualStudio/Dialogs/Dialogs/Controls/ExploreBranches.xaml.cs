using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using LaubPlusCo.Foundation.Helix.TemplateRepository;
using LaubPlusCo.Foundation.Helix.TemplateRepository.Services;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs.Controls
{
  /// <summary>
  ///   Interaction logic for ExploreBranches.xaml
  /// </summary>
  public partial class ExploreBranches : UserControl
  {
    protected readonly HelixTemplatesBranchService BranchService;

    public ExploreBranches() : this(Constants.GithubApiBaseUrl, Constants.DefaultRepositoryRelativeUrl)
    {
    }

    public ExploreBranches(string apiUrl, string repoRelativeUrl)
    {
      InitializeComponent();
      BranchService = new HelixTemplatesBranchService(apiUrl, repoRelativeUrl);
      LoadBranchList();
    }

    private void LoadBranchList()
    {
      if (!BranchService.IsReady)
        // Show error message
        return;
      InitializeBranchList();
    }

    private void InitializeBranchList()
    {
      BranchTypesPanel.Children.Clear();
      var branchTypes = BranchService.Branches.Select(b => b.Type).Distinct();

      foreach (var branchType in branchTypes)
      {
        var branches = BranchService.Branches.Where(b => b.Type.Equals(branchType)).ToArray();
        BranchTypesPanel.Children.Add(new BranchType(GetDisplayName(branchType), branches));
      }
    }

    private string GetDisplayName(string name)
    {
      return !string.IsNullOrEmpty(name) ?
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower()) : string.Empty;
    }
  }
}