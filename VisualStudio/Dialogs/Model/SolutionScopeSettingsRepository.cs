using System.IO;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class SolutionScopeSettingsRepository
  {
    protected string SolutionRootDirectory { get; }
    protected string ConfigurationFilePath { get; }

    protected const string ConfigurationFileName = ".helixtemplates";

    public SolutionScopeSettingsRepository(string solutionRoot)
    {
      SolutionRootDirectory = (solutionRoot.EndsWith("\\") ? solutionRoot : $"{solutionRoot}\\").ToLowerInvariant();
      ConfigurationFilePath = $"{solutionRoot}{ConfigurationFileName}";
    }

    public SolutionScopeTemplateSettings Get()
    {
      return !File.Exists(ConfigurationFilePath) ? null : ReadConfigFile();
    }

    protected virtual SolutionScopeTemplateSettings ReadConfigFile()
    {
      return new SolutionScopeTemplateSettings(File.ReadAllLines(ConfigurationFilePath));
    }

    public SolutionScopeTemplateSettings CreateSettingsFile(string templatesFolderPath, bool skipDialog = false)
    {
      var settings = Get() ?? new SolutionScopeTemplateSettings();
      templatesFolderPath = templatesFolderPath.ToLowerInvariant();
      templatesFolderPath = templatesFolderPath.StartsWith(SolutionRootDirectory)
        ? templatesFolderPath.Replace(SolutionRootDirectory, @"\") : templatesFolderPath;
      settings.ModuleTemplatesFolder = $"{templatesFolderPath}";
      settings.SkipCreateFolderDialog = skipDialog;
      WriteConfigFile(ConfigurationFilePath, settings);
      return settings;
    }

    protected virtual void WriteConfigFile(string configurationFilePath, SolutionScopeTemplateSettings configuration)
    {
      File.WriteAllText(configurationFilePath, configuration.ToString());
    }
  }
}