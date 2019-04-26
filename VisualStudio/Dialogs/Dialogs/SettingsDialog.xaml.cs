using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.PlatformUI;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for SettingsDialog.xaml
  /// </summary>
  public partial class SettingsDialog : DialogWindow
  {
    public SettingsDialog()
    {
      InitializeComponent();
      RootDirectoryPath.Text = GetCurrentRootDirectory();
      this.SetVisualStudioThemeStyles();
    }

    public string RootDirectory => RootDirectoryPath.Text;

    private string GetCurrentRootDirectory()
    {
      var rootDirectory = AppScopeSettingsRepository.GetGlobalRootDirectory();
      if (!string.IsNullOrEmpty(rootDirectory))
        return rootDirectory;
      MessageBox.Show("Please select where you want your Helix module and solution templates stored.\n\nIf you already selected a location please restart Visual Studio as administrator.", "Welcome", MessageBoxButton.OK);
      return AppScopeSettingsRepository.DefaultRootPath;
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
      var selectedRootpath = RootDirectoryPath.Text;
      if (string.IsNullOrEmpty(selectedRootpath))
      {
        MessageBox.Show(this, "It is required to select a root directory.", "Error", MessageBoxButton.OK);
        return;
      }
      if (!Directory.Exists(selectedRootpath))
      {
        var confirmResult = MessageBox.Show("Directory does not exist. Do you want to create it?", "Confirm", MessageBoxButton.YesNo);
        if (confirmResult != MessageBoxResult.Yes)
          return;
        Directory.CreateDirectory(selectedRootpath);
      }
      if (!AppScopeSettingsRepository.SetGlobalRootDirectory(selectedRootpath))
      {
        MessageBox.Show("Could not save settings. Please ensure that you are running this Visual Studio instance as administrator.", "Error", MessageBoxButton.OK);
        return;
      }
      if (!RootHasTemplateManifests(selectedRootpath))
      {
        var confirmResult = MessageBox.Show("Selected root does not contain templates.\n\nDo you want to install the default templates (recommended)?", "Confirm", MessageBoxButton.YesNo);
        if (confirmResult != MessageBoxResult.Yes)
          return;
        BuiltInTemplatesService.UnzipAll(selectedRootpath);
      }
      DialogResult = true;
      Close();
    }

    private bool RootHasTemplateManifests(string selectedRootpath)
    {
      return new DirectoryInfo(selectedRootpath).EnumerateFiles("template.manifest.xml", SearchOption.AllDirectories).Any();
    }

    private void UnpackTemplates_Clicked(object sender, RoutedEventArgs e)
    {
      var rootDirectory = AppScopeSettingsRepository.GetGlobalRootDirectory();
      if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
      { 
        MessageBox.Show(this, "You need to set a valid root directory.", "Error", MessageBoxButton.OK);
        return;
      }
      UnpackBuiltInButton.IsEnabled = false;
      if (Directory.EnumerateDirectories(rootDirectory).Any())
      {
        var overwriteConfirmResult = MessageBox.Show("This will overwrite changes made to built-in templates in this root folder.\nAre you sure you want to continue?\n\nNote: Always make your modifications changes in copies and never directly in the built-in example templates.", "Confirm", MessageBoxButton.YesNo);
        if (overwriteConfirmResult != MessageBoxResult.Yes)
        {
          UnpackBuiltInButton.IsEnabled = true;
          return;
        }
        BuiltInTemplatesService.DeleteExistingTemplates(rootDirectory);
      }
      BuiltInTemplatesService.UnzipAll(rootDirectory);
      MessageBox.Show("Built-in templates updated", "", MessageBoxButton.OK);
      UnpackBuiltInButton.IsEnabled = true;
    }
  }
}