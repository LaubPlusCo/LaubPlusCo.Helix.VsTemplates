using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LaubPlusCo.Foundation.HelixTemplating.Data;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class CopyTemplateObjectFilesService
  {
    private readonly List<ITemplateObject> _templateObjects;

    public CopyTemplateObjectFilesService(List<ITemplateObject> templateObjects)
    {
      _templateObjects = templateObjects;
    }

    public string[] Copy()
    {
      return Copy(_templateObjects);
    }

    protected virtual string[] Copy(IEnumerable<ITemplateObject> templateObjects)
    {
      var copiedFilePaths = new List<string>();
      foreach (var templateObject in templateObjects)
      {
        if (templateObject.IsIgnored)
          continue;
        if (templateObject.Type != TemplateObjectType.File && templateObject.Type != TemplateObjectType.Project)
        {
          if (!CreateDirectory(templateObject))
            throw new IOException("Could not create directory " + templateObject.DestinationFullPath);
          if (templateObject.ChildObjects.Any())
            copiedFilePaths.AddRange(Copy(templateObject.ChildObjects));
          continue;
        }
        copiedFilePaths.Add(CopyFile(templateObject));
      }
      return copiedFilePaths.ToArray();
    }

    protected virtual bool CreateDirectory(ITemplateObject templateObject)
    {
      var directory = Directory.CreateDirectory(templateObject.DestinationFullPath);
      return templateObject.IsCreated = directory.Exists;
    }

    protected virtual string CopyFile(ITemplateObject templateObject)
    {
      File.Copy(templateObject.OriginalFullPath, templateObject.DestinationFullPath, true);
      templateObject.IsCreated = true;
      return templateObject.DestinationFullPath;
    }
  }
}