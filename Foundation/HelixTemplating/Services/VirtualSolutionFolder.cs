using System.Collections.Generic;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class VirtualSolutionFolder
  {
    public string Name { get; set; }
    public IList<string> Files { get; set; }
    public IList<VirtualSolutionFolder> SubFolders { get; set; }

  }
}