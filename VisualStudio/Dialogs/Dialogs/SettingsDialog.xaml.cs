using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for SettingsDialog.xaml
  /// </summary>
  public partial class SettingsDialog : DialogWindow
  {
    protected readonly string SolutionRootDirectory;
    protected readonly bool IsSolutionCreation;
    protected readonly SolutionScopeSettings SolutionScopeSettings;

    public SettingsDialog() : this(string.Empty, false)
    {
    }

    public SettingsDialog(string solutionRootDirectory, bool isSolutionCreation)
    {
      SolutionRootDirectory = solutionRootDirectory;
      IsSolutionCreation = isSolutionCreation;
      SolutionScopeSettings = IsSolutionCreation
        ? null
        : new SolutionScopeSettings(solutionRootDirectory);

      InitializeComponent();
      SolutionSettingsTab.IsEnabled = !IsSolutionCreation;
      InitializeUpdateTemplatesButton();
      LoadSettings();
      this.SetVisualStudioThemeStyles();
      InitializeStyles();
      SetAboutTexts();
    }

    private void SetAboutTexts()
    {
      AboutVersionText.Text = $"version {AppScopeSettings.Current.InstalledVersion}";
    }

    public string GlobalTemplateFolder => GlobalTemplatesFolderTextbox.Text;

    protected string TemplateZipUrl => !string.IsNullOrEmpty(DownloadUrl.Text)
      ? DownloadUrl.Text
      : AppScopeSettings.Current.DownloadUrl;

    private void LoadSettings()
    {
      GlobalTemplatesFolderTextbox.Text = AppScopeSettings.Current.TemplatesFolder;
      ShowVsTokens.IsChecked = AppScopeSettings.Current.ShowVsTokensTab;
      DownloadTemplates.IsChecked = AppScopeSettings.Current.DownloadTemplates;
      DownloadUrl.Text = AppScopeSettings.Current.DownloadUrl;
    }

    private void InitializeStyles()
    {
      SettingTabs.Background = Background;
      SettingTabs.Style = Style;
      SettingTabs.BorderThickness = new Thickness(0, 2, 0, 0);
      SettingTabs.BorderBrush = (Brush) FindResource(VsBrushes.PanelHyperlinkKey);
      SettingTabs.Margin = new Thickness(0, 5, 15, 5);

      GlobalSettingsTab.Header = new Label
      {
        Content = "Global",
        Foreground = (Brush)FindResource(VsBrushes.CaptionTextKey),
        FontFamily = (FontFamily)FindResource(VsFonts.EnvironmentFontFamilyKey),
        Style = (Style)FindResource(VsResourceKeys.LabelEnvironment133PercentFontSizeStyleKey)
      };

      SolutionSettingsTab.Header = new Label
      {
        Content = "Solution",
        Foreground =  (Brush)FindResource(!IsSolutionCreation ? VsBrushes.CaptionTextKey : VsBrushes.InactiveCaptionTextKey),
        FontFamily = (FontFamily)FindResource(VsFonts.EnvironmentFontFamilyKey),
        Style = (Style)FindResource(VsResourceKeys.LabelEnvironment133PercentFontSizeStyleKey)
      };

      AboutTab.Header = new Label
      {
        Content = "About",
        Foreground = (Brush)FindResource(VsBrushes.CaptionTextKey),
        FontFamily = (FontFamily)FindResource(VsFonts.EnvironmentFontFamilyKey),
        Style = (Style)FindResource(VsResourceKeys.LabelEnvironment133PercentFontSizeStyleKey)
      };
    }

    private void InitializeUpdateTemplatesButton()
    {
      if (string.IsNullOrEmpty(AppScopeSettings.Current.TemplatesFolder) ||
          !Directory.Exists(AppScopeSettings.Current.TemplatesFolder))
      {
        UnpackBuiltInButton.Visibility = Visibility.Hidden;
        InstallTemplatesLabel.Visibility = Visibility.Hidden;
        return;
      }

      if (Directory.Exists(AppScopeSettings.Current.TemplatesFolder)
          && Directory.GetDirectories(AppScopeSettings.Current.TemplatesFolder)
            .Any(d => Directory.GetFiles(d).Any(f => f.EndsWith("template.manifest.xml"))))
      {
        InstallTemplatesLabel.Text = "Update templates in Global folder";
        return;
      }

      InstallTemplatesLabel.Text = "Install templates in Global folder";

      DownloadTemplates.IsChecked = AppScopeSettings.Current.DownloadTemplates;
      DownloadUrl.Text = AppScopeSettings.Current.DownloadUrl;
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
      if (!ValidateInput())
        return;

      if (!SaveSettings())
      {
        MessageBox.Show(
          "Could not save settings.\n\nPlease ensure folder paths are valid. If the problem persist, restart Visual Studio as administrator.",
          "Error", MessageBoxButton.OK);
        return;
      }

      if (!FolderHasTemplateManifests(GlobalTemplatesFolderTextbox.Text) && !ConfirmInstall())
        return;

      DialogResult = true;
      Close();
    }

    private bool ConfirmInstall()
    {
      var confirmResult =
        MessageBox.Show(
          "Selected folder does not contain any templates.\n\nDo you want to install the default example templates (recommended)?",
          "Confirm", MessageBoxButton.YesNo);
      if (confirmResult != MessageBoxResult.Yes)
        return false;
      InstallTemplates(GlobalTemplatesFolderTextbox.Text);
      return true;
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrEmpty(GlobalTemplatesFolderTextbox.Text))
      {
        MessageBox.Show(this, "It is required to select a global template folder.", "Error", MessageBoxButton.OK);
        return false;
      }

      if (Directory.Exists(GlobalTemplatesFolderTextbox.Text))
        return true;
      var confirmResult = MessageBox.Show("Folder does not exist. Do you want to create it?", "Confirm",
        MessageBoxButton.YesNo);
      if (confirmResult != MessageBoxResult.Yes)
        return false;
      Directory.CreateDirectory(GlobalTemplatesFolderTextbox.Text);

      //TODO: Validate download url.
      return true;
    }

    private bool SaveSettings()
    {
      if (SolutionScopeSettings != null
          && !string.IsNullOrEmpty(SolutionTemplatesFolderTextbox.Text))
      {
        var templateFolderService = new TemplateFolderService(SolutionRootDirectory);
        if (!templateFolderService.TryGetAbsolutePath(SolutionTemplatesFolderTextbox.Text, out var fullPath))
          return false;
        SolutionScopeSettings.RelativeTemplatesFolder = SolutionTemplatesFolderTextbox.Text;
        SolutionScopeSettings.SaveSettings();
      }

      AppScopeSettings.Current.ShowVsTokensTab = ShowVsTokens.IsChecked.HasValue && ShowVsTokens.IsChecked.Value;
      AppScopeSettings.Current.TemplatesFolder = GlobalTemplatesFolderTextbox.Text;
      AppScopeSettings.Current.DownloadTemplates =
        DownloadTemplates.IsChecked.HasValue && DownloadTemplates.IsChecked.Value;
      AppScopeSettings.Current.DownloadUrl = DownloadUrl.Text;
      return AppScopeSettings.Current.SaveSettings();
    }

    private bool FolderHasTemplateManifests(string selectedRootpath)
    {
      return new DirectoryInfo(selectedRootpath).EnumerateFiles("template.manifest.xml", SearchOption.AllDirectories)
        .Any();
    }

    private void UnpackTemplates_Clicked(object sender, RoutedEventArgs e)
    {
      var rootDirectory = AppScopeSettings.Current.TemplatesFolder;
      if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
      {
        MessageBox.Show(this, "You need to set a valid root directory.", "Error", MessageBoxButton.OK);
        return;
      }

      InstallTemplates(rootDirectory);
    }

    protected bool InstallTemplates(string targetFolder)
    {
      FileStorageService.Instance.CleanTempFolder();
      UnpackBuiltInButton.IsEnabled = false;
      var zipFilePath = string.Empty;
      if (DownloadTemplates.IsChecked.HasValue && DownloadTemplates.IsChecked.Value)
      {
        var downloadService = new DownloadFileService();
        if (!downloadService.TryDownloadFromUrl(TemplateZipUrl, out zipFilePath))
        {
          MessageBox.Show($"Downloading from:\n{TemplateZipUrl} failed.\n\n{downloadService.Message} ");
          UnpackBuiltInButton.IsEnabled = true;
          return false;
        }
      }

      if (Directory.EnumerateDirectories(targetFolder).Any())
      {
        var overwriteConfirmResult = MessageBox.Show(
          "This will overwrite any templates with matching folder names in this root folder.\nAre you sure you want to continue?\n\nNote: Always make your template modifications in copied folders and never directly in the original example templates.",
          "Confirm", MessageBoxButton.YesNo);
        if (overwriteConfirmResult != MessageBoxResult.Yes)
        {
          UnpackBuiltInButton.IsEnabled = true;
          return false;
        }
      }

      var templateInstallService = new TemplateInstallService(targetFolder, zipFilePath);
      var success = templateInstallService.Install();
      MessageBox.Show(success ? "Templates installed" : "Could not install templates.", "", MessageBoxButton.OK);
      UnpackBuiltInButton.IsEnabled = true;
      FileStorageService.Instance.RemoveTempFolder();
      return success;
    }


    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
    }
  }
}