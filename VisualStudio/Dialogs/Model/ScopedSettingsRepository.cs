using System;
using System.Diagnostics;
using System.IO;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class ScopedSettingsRepository
  {
    protected const string ConfigurationFileName = ".helixtemplates";
    protected readonly string ConfigurationFilePath;
    protected readonly string ScopeRootFolder;

    public ScopedSettingsRepository(string scopeRootFolder)
    {
      ScopeRootFolder = scopeRootFolder;
      ConfigurationFilePath = Path.Combine(scopeRootFolder, ConfigurationFileName);
    }

    public bool SettingsFileExists => File.Exists(ConfigurationFilePath);

    public ScopedTemplateSettings Get()
    {
      return !SettingsFileExists ? new ScopedTemplateSettings() : ReadConfigFile();
    }

    protected virtual ScopedTemplateSettings ReadConfigFile()
    {
      return new ScopedTemplateSettings(File.ReadAllLines(ConfigurationFilePath));
    }

    public virtual bool WriteConfigFile(ScopedTemplateSettings settings)
    {
      try
      {
        File.WriteAllText(ConfigurationFilePath, settings.ToString());
      }
      catch (Exception e)
      {
        Trace.WriteLine(e.Message);
        return false;
      }

      return true;
    }
  }
}