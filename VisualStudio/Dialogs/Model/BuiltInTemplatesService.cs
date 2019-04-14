using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Properties;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class BuiltInTemplatesService
  {
    private const string DefaultTemplateFolderName = "HelixTemplates";

    public static string CreateTemplateFolder(string rootPath, string folderName = DefaultTemplateFolderName)
    {
      if (!Directory.Exists(rootPath))
        throw new ArgumentException("Root path does not exist, cannot create template folder");
      var templateFolderPath = Path.Combine(rootPath, folderName);
      if (!Directory.Exists(templateFolderPath))
        Directory.CreateDirectory(templateFolderPath);
      return templateFolderPath;
    }

    public static void Unzip(string rootpath, TemplateType templateType)
    {
      var tempFile = Path.GetTempFileName();
      File.WriteAllBytes(tempFile,
        templateType == TemplateType.Module ? Resources.ModuleTemplates : Resources.SolutionTemplates);
      ZipFile.ExtractToDirectory(tempFile, rootpath);
    }

    public static void DeleteExistingTemplates(string rootDirectory)
    {
      var directories = new List<string>();
      directories.AddRange(ReadTemplateFolderNames(TemplateType.Module));
      directories.AddRange(ReadTemplateFolderNames(TemplateType.Solution));

      foreach (var directory in directories)
      {
        var dieDirectory = Path.GetFullPath(Path.Combine(rootDirectory, directory));
        if (!Directory.Exists(dieDirectory))
          continue;
        Directory.Delete(dieDirectory, true);
      }
    }

    private static IEnumerable<string> ReadTemplateFolderNames(TemplateType type)
    {
      using (var zipArchive = new ZipArchive(new MemoryStream(type == TemplateType.Solution
        ? Resources.SolutionTemplates
        : Resources.ModuleTemplates), ZipArchiveMode.Read))
      {
        return zipArchive.Entries.Select(e => e.FullName.Split('\\')[0]).Distinct().ToArray();
      }
    }

    public static void UnzipAll(string selectedRootpath)
    {
      Unzip(selectedRootpath, TemplateType.Solution);
      Unzip(selectedRootpath, TemplateType.Module);
    }
  }
}