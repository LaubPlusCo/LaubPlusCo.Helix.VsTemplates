using System;
using System.IO;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class SolutionScopeSettings
  {
    protected const string TemplatesFolderKey = "templates.modules.folder";
    protected const string SkipCreateFolderDialogKey = "templates.modules.skipcreatedialog";
    protected const string DownloadFromUrlKey = "templates.modules.downloadurl";
    protected const string DefaultRootPath = @".\HelixTemplates\";
    protected const string DefaultDownloadUrl = "https://github.com/LaubPlusCo/Helix-Templates/archive/master.zip";
    protected readonly string SolutionRootPath;
    public readonly bool ValidContext;

    private string _relativeTemplatesFolder;

    protected ScopedSettingsRepository SettingsRepository;

    public SolutionScopeSettings(string solutionRootPath)
    {
      SolutionRootPath = solutionRootPath.ToLowerInvariant();
      if (string.IsNullOrEmpty(SolutionRootPath) || !Directory.Exists(SolutionRootPath))
      {
        ValidContext = false;
        return;
      }

      ValidContext = true;
      SettingsRepository = new ScopedSettingsRepository(SolutionRootPath);
      InitializeSettings();
    }

    public string RelativeTemplatesFolder
    {
      get => _relativeTemplatesFolder;
      set
      {
        if (value == null)
          return;
        _relativeTemplatesFolder = value.StartsWith(SolutionRootPath, StringComparison.OrdinalIgnoreCase)
          ? value.ToLowerInvariant().Replace(SolutionRootPath, ".\\")
          : value;
      }
    }

    protected string AbsoluteTemplatesFolder =>
      Path.Combine(SolutionRootPath, RelativeTemplatesFolder.Replace(".\\", "").Replace("./", ""));

    public string DownloadUrl { get; set; }
    public bool SkipCreateFolderDialog { get; set; }

    public bool SaveSettings()
    {
      if (!Directory.Exists(AbsoluteTemplatesFolder))
        return false;
      var settings = SettingsRepository.Get();

      settings.Set(TemplatesFolderKey, RelativeTemplatesFolder);
      settings.Set(DownloadFromUrlKey, DownloadUrl);
      settings.Set(SkipCreateFolderDialogKey, SkipCreateFolderDialog);

      return SettingsRepository.WriteConfigFile(settings);
    }

    protected void InitializeSettings()
    {
      var settings = SettingsRepository.Get();
      RelativeTemplatesFolder = settings.GetSetting(TemplatesFolderKey, DefaultRootPath);
      SkipCreateFolderDialog = settings.GetBooleanSetting(SkipCreateFolderDialogKey);
      DownloadUrl = settings.GetSetting(DownloadFromUrlKey, DefaultDownloadUrl);
    }
  }
}