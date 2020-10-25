using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE100;
using EnvDTE80;

namespace LaubPlusCo.VisualStudio.Helix.Wizard.Repositories
{
  public class SolutionFolderRepository
  {
    protected const string VsProjectKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
    protected Solution4 Solution;

    public SolutionFolderRepository(Solution4 solution)
    {
      Solution = solution;
    }

    public virtual SolutionFolder GetByName(string folderName)
    {
      var allFolders = GetAll();
      return allFolders.FirstOrDefault(f => f.Parent.Name.Equals(folderName, StringComparison.InvariantCultureIgnoreCase));
    }

    public virtual IEnumerable<SolutionFolder> GetAll()
    {
      var solutionProjects = Solution.Projects.Cast<object>().Select(o => o as Project).Where(p => p != null);
      return GetFolders(solutionProjects, new List<SolutionFolder>());
    }

    private IEnumerable<SolutionFolder> GetFolders(IEnumerable<Project> projects, List<SolutionFolder> solutionFolders)
    {
      projects = projects.Where(p => p != null).Where(p => p.Kind == VsProjectKindSolutionFolder && p.Object != null).ToArray();
      foreach (var project in projects)
      {
        var folder = (SolutionFolder)project.Object;
        if (folder == null)
          continue;
        solutionFolders.Add(folder);
        if (project.ProjectItems == null || project.ProjectItems.Count <= 0)
          continue;
        GetFolders(project.ProjectItems.Cast<ProjectItem>().Where(pi => pi.SubProject != null).Select(pi => pi.SubProject), solutionFolders);
      }
      return solutionFolders;
    }

    public string Create(string folderName)
    {
      var folder = GetByName(folderName);
      if (folder != null)
        return folderName;
      Solution.AddSolutionFolder(folderName);
      return folderName;
    }

    public string Create(string folderName, SolutionFolder parentFolder)
    {
      var folder = GetByName(folderName);
      if (folder != null)
        return folderName;
      parentFolder.AddSolutionFolder(folderName);
      return folderName;
    }
  }
}