/* Sitecore Helix Visual Studio Templates 
 * 
 * Copyright (C) 2021, Anders Laub - Laub plus Co, DK 29 89 76 54 contact@laubplusco.net https://laubplusco.net
 * 
 * Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby granted, 
 * provided that the above copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING 
 * ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, 
 * DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, 
 * WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE 
 * OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LaubPlusCo.Foundation.HelixTemplating.Data;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;

namespace LaubPlusCo.Foundation.HelixTemplating.TemplateEngine
{
  public class DefaultHelixTemplateEngine : IHelixTemplateEngine
  {
    protected virtual HelixTemplateManifest Manifest { get; set; }
    protected virtual ReplaceTokensService ReplaceTokensService { get; set; }
    protected virtual BuildDestinationPathService BuildDestinationPathService { get; set; }
    protected virtual string DestinationRootPath { get; set; }

    public IHelixProjectTemplate Run(HelixTemplateManifest manifest, string solutionRootPath)
    {
      Manifest = manifest;
      WriteTraceService.WriteToTrace("Template Engine started", "\nInfo", $"Manifest: {manifest.Name}",
        $"root path: {solutionRootPath} ");
      ReplaceTokensService = new ReplaceTokensService(Manifest.ReplacementTokens);
      DestinationRootPath = solutionRootPath;
      BuildDestinationPathService = new BuildDestinationPathService(Manifest.ManifestRootPath, DestinationRootPath);
      ExpandConditions();

      var templateObjects = new List<ITemplateObject>();
      templateObjects.AddRange(GetTemplateObjectFromDirectory(Manifest.ManifestRootPath));
      var copyTemplateObjectsService = new CopyTemplateObjectFilesService(templateObjects);
      var copiedFilePaths = copyTemplateObjectsService.Copy();

      if (!copiedFilePaths.Any())
        return new HelixProjectTemplate
        {
          Manifest = Manifest,
          TemplateObjects = templateObjects,
          ReplacementTokens = Manifest.ReplacementTokens
        };

      var replaceFileTokensService = new ReplaceTokensInFilesService(copiedFilePaths, Manifest.ReplacementTokens);
      replaceFileTokensService.Replace();
      EvaluateSkipAttach(templateObjects);
      CreateVirtualSolutionFolders(templateObjects);

      return new HelixProjectTemplate
      {
        Manifest = Manifest,
        TemplateObjects = templateObjects,
        ReplacementTokens = Manifest.ReplacementTokens
      };
    }

    protected virtual void ExpandConditions()
    {
      ExpandConditionTokens(Manifest.IgnorePaths);
      ExpandConditionTokens(Manifest.ProjectsToAttach);
      ExpandConditionTokens(Manifest.SkipAttachPaths);
      Manifest.IgnorePaths = Manifest.IgnorePaths.Where(cv => cv.Evaluate()).ToList();
      Manifest.ProjectsToAttach = Manifest.ProjectsToAttach.Where(cv => cv.Evaluate()).ToList();
      Manifest.SkipAttachPaths = Manifest.SkipAttachPaths.Where(cv => cv.Evaluate()).ToList();
    }

    protected void ExpandConditionTokens(IList<ConditionalValue> conditionalValues)
    {
      foreach (var conditionalValue in conditionalValues.Where(cv => !string.IsNullOrEmpty(cv.Condition)))
        conditionalValue.Condition = ReplaceTokensService.Replace(conditionalValue.Condition);
    }

    protected virtual void CreateVirtualSolutionFolders(IList<ITemplateObject> templateObjects)
    {
      if (Manifest.VirtualSolutionFolders == null || !Manifest.VirtualSolutionFolders.Any())
        return;
      var sourceRootObject = FindSourceRootTemplateObjectService.Find(templateObjects);
      GetVirtualSolutionFolderTemplateObjects(sourceRootObject, Manifest.VirtualSolutionFolders,
        Path.Combine(sourceRootObject.DestinationFullPath));
    }

    protected virtual void GetVirtualSolutionFolderTemplateObjects(ITemplateObject root,
      IList<VirtualSolutionFolder> virtualSolutionFolders, string parentPath)
    {
      foreach (var virtualSolutionFolder in virtualSolutionFolders)
      {
        var virtualFolderObject = new TemplateObject
        {
          Type = TemplateObjectType.Folder,
          DestinationFullPath = Path.Combine(parentPath, virtualSolutionFolder.Name)
        };

        foreach (var filePath in virtualSolutionFolder.Files)
          virtualFolderObject.ChildObjects.Add(new TemplateObject
          {
            ChildObjects = null,
            Type = TemplateObjectType.File,
            OriginalFullPath = filePath,
            DestinationFullPath = ReplaceTokensService.Replace(BuildDestinationPathService.Build(filePath))
          });

        if (virtualSolutionFolder.SubFolders == null || !virtualSolutionFolders.Any())
          continue;
        GetVirtualSolutionFolderTemplateObjects(virtualFolderObject, virtualSolutionFolder.SubFolders,
          virtualFolderObject.DestinationFullPath);
        root.ChildObjects.Add(virtualFolderObject);
      }
    }

    protected virtual void EvaluateSkipAttach(IList<ITemplateObject> templateObjects)
    {
      foreach (var templateObject in templateObjects)
      {
        templateObject.SkipAttach = SkipAttach(templateObject);
        if (templateObject.ChildObjects == null || !templateObject.ChildObjects.Any())
          continue;
        if (templateObject.SkipAttach)
        {
          SetSkipAttachFlag(templateObject.ChildObjects);
          continue;
        }

        EvaluateSkipAttach(templateObject.ChildObjects);
      }
    }

    protected virtual void SetSkipAttachFlag(IEnumerable<ITemplateObject> skipAttachTemplateObjects)
    {
      foreach (var templateObject in skipAttachTemplateObjects)
      {
        templateObject.SkipAttach = templateObject.Type != TemplateObjectType.Project;
        if (templateObject.ChildObjects == null || !templateObject.ChildObjects.Any())
          continue;
        SetSkipAttachFlag(templateObject.ChildObjects);
      }
    }

    protected virtual IList<ITemplateObject> GetTemplateObjectFromDirectory(string directoryPath)
    {
      var templateObjects = Directory.EnumerateFiles(directoryPath).Select(GetTemplateObjectFromFile)
        .Where(objectFromFile => objectFromFile != null).ToList();
      templateObjects.AddRange(Directory.EnumerateDirectories(directoryPath)
        .Select(directory => new TemplateObject
        {
          Type = IsSourceRoot(directory) ? TemplateObjectType.SourceRoot : TemplateObjectType.Folder,
          ChildObjects = GetTemplateObjectFromDirectory(directory),
          OriginalFullPath = directory,
          DestinationFullPath = ReplaceTokensService.Replace(BuildDestinationPathService.Build(directory))
        }));
      return templateObjects;
    }

    protected virtual ITemplateObject GetTemplateObjectFromFile(string filePath)
    {
      var isIgnored = IsIgnored(filePath);
      return new TemplateObject
      {
        Type = IsProjectToAttach(filePath) ? TemplateObjectType.Project : TemplateObjectType.File,
        ChildObjects = null,
        OriginalFullPath = filePath,
        IsIgnored = isIgnored,
        DestinationFullPath = isIgnored ? "" : ReplaceTokensService.Replace(BuildDestinationPathService.Build(filePath))
      };
    }

    protected virtual bool IsIgnored(string filePath)
    {
      return IsFileNameInList(filePath, Manifest.IgnorePaths) || Manifest.IgnorePaths.Select(i => i.Value)
               .Any(ignore => filePath.Equals(ignore, StringComparison.OrdinalIgnoreCase) ||
                              filePath.StartsWith(ignore, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual bool SkipAttach(ITemplateObject templateObject)
    {
      return templateObject.Type != TemplateObjectType.Project &&
             (SkipPath(templateObject.OriginalFullPath) || templateObject.ChildObjects != null
              && templateObject.ChildObjects.Any(c => c.Type == TemplateObjectType.Project));
    }

    protected virtual bool SkipPath(string path)
    {
      return IsFileNameInList(path, Manifest.SkipAttachPaths) || Manifest.SkipAttachPaths.Any(skipPath =>
               skipPath.Value.Equals(path, StringComparison.InvariantCultureIgnoreCase));
    }

    protected bool IsFileNameInList(string path, IList<ConditionalValue> pathList)
    {
      var fileName = Path.GetFileName(path);
      if (string.IsNullOrEmpty(fileName)) return false;

      //TODO: Implement glob support and expand paths and merge lists rather than loop-i-loop n^2*n^2..
      var fileNames = pathList.Where(cv =>
          cv.Value.Length > 0 && cv.Value.IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0
                              && cv.Value.IndexOf("\\", StringComparison.OrdinalIgnoreCase) < 0)
        .Select(skipFile => skipFile.Value);

      return fileNames.Any(skipFile => skipFile.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual bool IsSourceRoot(string path)
    {
      return Manifest.SourceFolder.Equals(path, StringComparison.InvariantCultureIgnoreCase);
    }

    protected virtual bool IsProjectToAttach(string filePath)
    {
      return Manifest.ProjectsToAttach.Any(projectPath =>
        projectPath.Value.Equals(filePath, StringComparison.InvariantCultureIgnoreCase));
    }
  }
}