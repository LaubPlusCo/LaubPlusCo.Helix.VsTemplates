using System.Collections.Generic;
using System.Threading;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs;

namespace LaubPlusCo.VisualStudio.Helix.Dialogs.TestApp
{
  class Program
  {
    private static readonly IDictionary<string, string> ReplacementTokens = new Dictionary<string, string>
      {
        { "$solutiondir$", "dsasadasd"},
        { "$projectsafename$", "Test"},
        { "$projectname$", "test test"},
      }
      ;

    static void Main(string[] args)
    {
      var dialogThread = new Thread(OpenDialog);
      dialogThread.SetApartmentState(ApartmentState.STA);
      dialogThread.Start();
    }

    private static void OpenDialog()
    {
      var helixManifestDialog = new ManifestDialog();
      helixManifestDialog.Initialize(@"c:\projects\Helix.Templates", @"c:\projects\TestFolder", ReplacementTokens, true);
      helixManifestDialog.ShowModal();
    }
  }
}
