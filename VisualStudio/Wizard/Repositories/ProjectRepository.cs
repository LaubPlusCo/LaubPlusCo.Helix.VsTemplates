using System;
using EnvDTE100;
using EnvDTE80;

namespace LaubPlusCo.VisualStudio.Helix.Wizard.Repositories
{
  public class ProjectRepository
  {
    protected Solution4 Solution;

    public ProjectRepository(Solution4 solution)
    {
      Solution = solution;
    }

    public void AddProjectFromFile(string filePath, SolutionFolder parentFolder)
    {
      try
      {
        parentFolder.AddFromFile(filePath);
      }
      catch (Exception innerException)
      {
        throw new ArgumentException($"Cannot add file {filePath} to solution folder {parentFolder.Parent.Name}.", innerException);
      }
    }

    public void AddFromFile(string filePath, SolutionFolder parentFolder)
    {
      try
      {
        var parentFolderProject = parentFolder.Parent;
        parentFolderProject.ProjectItems.AddFromFile(filePath);
      }
      catch (Exception innerException)
      {
      }
    }

    public void AddFromFile(string filePath)
    {
      Solution.AddFromFile(filePath);
    }
  }
}