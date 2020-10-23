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
    protected const string DefaultDownloadPath = "https://github.com/LaubPlusCo/Helix-Templates/archive/release/";

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
    public string DefaultDownloadUrl { get; set; }
    public string InstalledVersion { get; set; }
    public bool DownloadTemplates { get; set; }
    public bool ShowVsTokensTab { get; set; }
    public bool VersionNoticeShown { get; set; }

    private bool _justShownVersionNoticeShown;

    public bool IsFirstRun => !SettingsRepository.SettingsFileExists || !Directory.Exists(TemplatesFolder);

    public bool SaveSettings()
    {
      if (!Directory.Exists(TemplatesFolder))
        return false;

      var settings = SettingsRepository.Get();
      settings.Set(TemplatesFolderKey, TemplatesFolder);
      settings.Set(DownloadFromUrlKey, DownloadUrl);
      settings.Set(InstalledVersionKey, InstalledVersion);
      settings.Set(DownloadTemplatesKey, DownloadTemplates);
      settings.Set(ShowVsTokensTabKey, ShowVsTokensTab);
      if (!_justShownVersionNoticeShown)
        return SettingsRepository.WriteConfigFile(settings);

      settings.Set(VersionNoticeShownForKey, InstalledVersion);
      settings.Set(VersionNoticeShownKey, true);
      return SettingsRepository.WriteConfigFile(settings);
    }

    protected void InitializeSettings()
    {
      var settings = SettingsRepository.Get();
      var saveSettings = !SettingsRepository.SettingsFileExists;
      var currentVersion = VsixManifest.GetManifest().Version;
      var templateFolder = !SettingsRepository.SettingsFileExists ? LegacySettings.GetGlobalTemplateFolder() : "";
      TemplatesFolder = settings.GetSetting(TemplatesFolderKey,
        !string.IsNullOrEmpty(templateFolder) ? templateFolder : DefaultRootPath);

      InstalledVersion = settings.GetSetting(InstalledVersionKey, currentVersion);
      DefaultDownloadUrl = string.Concat(DefaultDownloadPath, $"v{currentVersion}.zip");
      DownloadUrl = settings.GetSetting(DownloadFromUrlKey, DefaultDownloadUrl);
      if (!DownloadUrl.Equals(DefaultDownloadUrl, StringComparison.Ordinal)
          && DownloadUrl.StartsWith(DefaultDownloadPath, StringComparison.OrdinalIgnoreCase))
      { 
        DownloadUrl = DefaultDownloadUrl;
        saveSettings = true;
      }

      DownloadTemplates = settings.GetBooleanSetting(DownloadTemplatesKey, true);
      ShowVsTokensTab = settings.GetBooleanSetting(ShowVsTokensTabKey, true);

      var shownNoticeVersion = settings.GetSetting(VersionNoticeShownForKey);
      VersionNoticeShown = settings.GetBooleanSetting(VersionNoticeShownKey) && shownNoticeVersion.Equals(InstalledVersion);

      if (saveSettings)
        SaveSettings();
    }

    public void SetVersionNoticeShown()
    {
      _justShownVersionNoticeShown = true;
      SaveSettings();
    }

    protected static ScopedSettingsRepository GetGlobalSettingsRepository()
    {
      return new ScopedSettingsRepository(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }
  }
}