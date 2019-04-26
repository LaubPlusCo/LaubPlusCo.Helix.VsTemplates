using System;
using System.IO;
using System.Linq;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class TemplateFolderService
  {
    protected string RootDirectory { get; }
    protected const string TemplateManifestFilenamePattern = "template.manifest.*";

    public TemplateFolderService(string root)
    {
      RootDirectory = root.EndsWith("\\") ? root : $"{root}\\";
    }

    public string Locate()
    {
      return SearchForTemplatesFolder() ?? string.Empty;
    }

    public bool TryGetAbsolutePath(string templateFolder, out string fullPath)
    {
      fullPath = string.Empty;
      if (string.IsNullOrEmpty(templateFolder))
        return false;

      fullPath = templateFolder.StartsWith(RootDirectory, StringComparison.InvariantCultureIgnoreCase)
        ? templateFolder
        : Path.Combine(RootDirectory, templateFolder);

      return Directory.Exists(fullPath)
             && Directory.GetFiles(fullPath, TemplateManifestFilenamePattern, SearchOption.TopDirectoryOnly)
               .Any();
    }

    protected virtual string SearchForTemplatesFolder()
    {
      return Directory.GetDirectories(RootDirectory)
        .Where(rd => !rd.EndsWith("node_modules") && !rd.EndsWith("src")).FirstOrDefault(IsTemplateFolder);
    }

    protected bool IsTemplateFolder(string folderPath)
    {
      return Directory.GetDirectories(folderPath).Any(d => Directory.GetFiles(d, TemplateManifestFilenamePattern, SearchOption.AllDirectories).Any());
    }
  }
}