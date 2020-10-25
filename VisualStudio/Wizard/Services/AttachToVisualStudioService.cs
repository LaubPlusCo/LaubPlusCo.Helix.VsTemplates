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
      AttachTemplateObject(sourceRoot, null);
    }

    private void AttachTemplateObject(ITemplateObject templateObject, string currentSolutionFolder)
    {
      if (templateObject.Type == TemplateObjectType.Folder 
          && templateObject.ChildObjects.All(c => c.Type != TemplateObjectType.Project))
      {
        currentSolutionFolder = AttachSolutionFolder(templateObject, currentSolutionFolder);
      }

      if (templateObject.ChildObjects == null)
        return;

      foreach (var childObject in templateObject.ChildObjects.Where(to => !to.IsIgnored))
      {
        if (childObject.Type == TemplateObjectType.File 
            || childObject.Type == TemplateObjectType.Project)
          Attach(childObject, currentSolutionFolder);

        if (childObject.Type == TemplateObjectType.Folder)
        {
          AttachTemplateObject(childObject, currentSolutionFolder);
        }
      }
    }

    protected virtual string AttachSolutionFolder(ITemplateObject templateObject, string parentFolderName = null)
    {
      if (templateObject.SkipAttach)
        return parentFolderName;

      if (string.IsNullOrEmpty(parentFolderName))
      {
        return SolutionFolderRepository.Create(templateObject.Name);
      }

      var parentFolder = SolutionFolderRepository.GetByName(parentFolderName);
      if (parentFolder == null)
        throw new ArgumentException($"Scaffolding failed. Cannot find parent solution folder called {parentFolderName} for {templateObject.Name}.");

      return SolutionFolderRepository.Create(templateObject.Name, parentFolder);
    }

    protected virtual void Attach(ITemplateObject templateObject, string parentFolderName = null)
    {
      if (templateObject.SkipAttach)
        return;

      if (string.IsNullOrEmpty(parentFolderName))
      {
        ProjectsRepository.AddFromFile(templateObject.DestinationFullPath);
        return;
      }

      var parentFolder = SolutionFolderRepository.GetByName(parentFolderName);
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