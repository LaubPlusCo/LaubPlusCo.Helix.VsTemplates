using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE100;
using EnvDTE80;
using LaubPlusCo.Foundation.HelixTemplating.Data;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;
using LaubPlusCo.VisualStudio.Helix.Wizard.Repositories;

namespace LaubPlusCo.VisualStudio.Helix.Wizard.Services
{
  public class AttachToVisualStudioService
  {
    protected const string VsProjectKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
    protected readonly DTE2 DteInterface;
    protected readonly ProjectRepository ProjectsRepository;
    protected readonly SolutionFolderRepository SolutionFolderRepository;

    public AttachToVisualStudioService(DTE2 dte)
    {
      DteInterface = dte;
      ProjectsRepository = new ProjectRepository((Solution4)dte.Solution);
      SolutionFolderRepository = new SolutionFolderRepository((Solution4)dte.Solution);
    }

    public void Attach(IHelixProjectTemplate projectTemplate)
    {
      var sourceRoot = FindSourceRootTemplateObjectService.Find(projectTemplate.TemplateObjects);
      if (sourceRoot == null) throw new ArgumentException("Missing a source root folder in Helix template - the start location for Visual Studio to attach files and folders from");
      AttachTemplateObjects(sourceRoot, new List<string>());
    }

    private void AttachTemplateObjects(ITemplateObject rootTemplateObject, IList<string> solutionLocation)
    {
      if (rootTemplateObject.ChildObjects.All(c => c.Type != TemplateObjectType.Project))
        solutionLocation.Add(rootTemplateObject.Name);

      foreach (var templateObject in rootTemplateObject.ChildObjects.Where(to => !to.IsIgnored && !to.IsProjectContent))
      {
        if (templateObject.Type == TemplateObjectType.Folder)
        {
          if (templateObject.ChildObjects == null || !templateObject.ChildObjects.Any())
          {
            EnsureSolutionFolder(templateObject, solutionLocation);
            continue;
          }
          if (templateObject.ChildObjects.All(t => t.Type != TemplateObjectType.Project))
            EnsureSolutionFolder(templateObject, solutionLocation);
          AttachTemplateObjects(templateObject, solutionLocation.ToList());
        }
        if (templateObject.Type == TemplateObjectType.File || templateObject.Type == TemplateObjectType.Project)
          AttachFile(templateObject, solutionLocation);
      }
    }

    protected virtual void EnsureSolutionFolder(ITemplateObject templateObject, IList<string> solutionLocation)
    {
      if (solutionLocation.Count == 1)
      {
        SolutionFolderRepository.Create(templateObject.Name);
        return;
      }
      var parentFolder = SolutionFolderRepository.GetByName(solutionLocation[solutionLocation.Count - 1]);
      if (parentFolder == null)
        throw new ArgumentException($"Scaffolding failed. Cannot find parent solution folder called {solutionLocation[solutionLocation.Count - 1]} for {templateObject.Name}.");
      SolutionFolderRepository.Create(templateObject.Name, parentFolder);
    }

    protected virtual void AttachFile(ITemplateObject templateObject, IList<string> solutionLocation)
    {
      if (!solutionLocation.Any())
        throw new ArgumentException("Miss solution root object");
      if (solutionLocation.Count == 1)
      {
        ProjectsRepository.AddFromFile(templateObject.DestinationFullPath);
        return;
      }
      var parentFolder = SolutionFolderRepository.GetByName(solutionLocation[solutionLocation.Count - 1]);
      if (parentFolder == null)
        throw new ArgumentException($"Scaffolding failed. Cannot find parent solution folder for file {templateObject.DestinationFullPath}.");
      if (templateObject.Type == TemplateObjectType.Project)
      {
        ProjectsRepository.AddProjectFromFile(templateObject.DestinationFullPath, parentFolder);
        return;
      }
      ProjectsRepository.AddFromFile(templateObject.DestinationFullPath, parentFolder);
    }
  }
}