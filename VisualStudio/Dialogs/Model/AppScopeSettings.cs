using System;
using System.IO;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class AppScopeSettings
  {
    protected const string TemplatesFolderKey = "global.folder";
    protected const string DownloadFromUrlKey = "global.downloadurl";
    protected const string InstalledVersionKey = "global.version";
    protected const string ShowVsTokensTabKey = "global.showvstokenstab";
    protected const string DownloadTemplatesKey = "global.downloadtemplates";
    protected const string VersionNoticeShownKey = "global.versionnoticeshown";
    protected const string VersionNoticeShownForKey = "global.versionnoticefor";
    protected const string DefaultRootPath = @"c:\projects\helix.templates\";
    protected const string DefaultDownloadUrl = "https://github.com/LaubPlusCo/Helix-Templates/archive/master.zip";

    private static AppScopeSettings _instance;

    protected ScopedSettingsRepository SettingsRepository;

    private AppScopeSettings()
    {
      SettingsRepository = GetGlobalSettingsRepository();
      InitializeSettings();
    }

    public static AppScopeSettings Current => _instance ?? (_instance = new AppScopeSettings());

    public string TemplatesFolder { get; set; }
    public string DownloadUrl { get; set; }
    public string InstalledVersion { get; set; }
    public bool DownloadTemplates { get; set; }
    public bool ShowVsTokensTab { get; set; }
    public bool VersionNoticeShown { get; set; }

    private bool _versionNoticeShown;

    public bool IsFirstRun => !SettingsRepository.SettingsFileExists || !Directory.Exists(TemplatesFolder);

    public bool SaveSettings(ScopedTemplateSettings settings = null)
    {
      if (!Directory.Exists(TemplatesFolder))
        return false;

      settings = SettingsRepository.Get();
      settings.Set(TemplatesFolderKey, TemplatesFolder);
      settings.Set(DownloadFromUrlKey, DownloadUrl);
      settings.Set(InstalledVersionKey, InstalledVersion);
      settings.Set(DownloadTemplatesKey, DownloadTemplates);
      settings.Set(ShowVsTokensTabKey, ShowVsTokensTab);
      if (!_versionNoticeShown)
        return SettingsRepository.WriteConfigFile(settings);

      settings.Set(VersionNoticeShownForKey, InstalledVersion);
      settings.Set(VersionNoticeShownKey, true);
      return SettingsRepository.WriteConfigFile(settings);
    }

    protected void InitializeSettings()
    {
      var settings = SettingsRepository.Get();
      var currentVersion = VsixManifest.GetManifest().Version;

      var templateFolder = !SettingsRepository.SettingsFileExists ? LegacySettings.GetGlobalTemplateFolder() : "";

      TemplatesFolder = settings.GetSetting(TemplatesFolderKey,
        !string.IsNullOrEmpty(templateFolder) ? templateFolder : DefaultRootPath);
      InstalledVersion = settings.GetSetting(InstalledVersionKey, currentVersion);
      DownloadUrl = settings.GetSetting(DownloadFromUrlKey, DefaultDownloadUrl);
      DownloadTemplates = settings.GetBooleanSetting(DownloadTemplatesKey, true);
      var shownNoticeVersion = settings.GetSetting(VersionNoticeShownForKey);
      ShowVsTokensTab = settings.GetBooleanSetting(ShowVsTokensTabKey, true);
      _versionNoticeShown = settings.GetBooleanSetting(VersionNoticeShownKey)
                           && shownNoticeVersion.Equals(InstalledVersion);

      if (!SettingsRepository.SettingsFileExists)
        SaveSettings();
    }

    public void SetVersionNoticeShown()
    {
      _versionNoticeShown = true;
      SaveSettings();
    }

    protected static ScopedSettingsRepository GetGlobalSettingsRepository()
    {
      return new ScopedSettingsRepository(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }
  }
}