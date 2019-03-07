using System.IO;
using System.Linq;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class ModuleTemplateFolderService
  {
    protected const string ConfigurationFileName = ".helixVsConfig";
    protected const string TemplateManifestFilenamePattern = "template.manifest.*";

    public ModuleTemplateFolderService(string solutionRoot)
    {
      SolutionRootDirectory = solutionRoot.EndsWith("\\") ? solutionRoot : $"{solutionRoot}\\";
      ConfigurationFilePath = $"{solutionRoot}{ConfigurationFileName}";
    }

    protected string SolutionRootDirectory { get; }
    protected string ConfigurationFilePath { get; }

    public string Locate()
    {
      var templateFolder = GetFromConfiguration();

      if (!string.IsNullOrEmpty(templateFolder)
          && Directory.Exists(templateFolder)
          && Directory.GetFiles(templateFolder, TemplateManifestFilenamePattern, SearchOption.TopDirectoryOnly).Any())
        return templateFolder;
      templateFolder = SearchForTemplatesFolder();
      if (templateFolder == null) return string.Empty;
      CreateConfigFile(templateFolder);
      return templateFolder;
    }

    protected virtual string GetFromConfiguration()
    {
      if (!File.Exists(ConfigurationFilePath)) return string.Empty;
      var config = ReadConfigFile();
      return config.ModuleTemplatesFolder;
    }

    protected virtual HelixTemplateConfiguration ReadConfigFile()
    {
      return new HelixTemplateConfiguration(File.ReadAllLines(ConfigurationFilePath));
    }

    protected void CreateConfigFile(string templatesFolder)
    {
      var config = new HelixTemplateConfiguration();
      config.ModuleTemplatesFolder = ConfigurationFilePath;
      WriteConfigFile(ConfigurationFilePath, config);
    }

    protected virtual void WriteConfigFile(string configurationFilePath, HelixTemplateConfiguration configuration)
    {
      File.WriteAllText(configurationFilePath, configuration.ToString());
    }

    protected virtual string SearchForTemplatesFolder()
    {
      return Directory.GetDirectories(SolutionRootDirectory)
        .FirstOrDefault(
          d => Directory.GetFiles(d, TemplateManifestFilenamePattern, SearchOption.TopDirectoryOnly).Any());
    }
  }
}