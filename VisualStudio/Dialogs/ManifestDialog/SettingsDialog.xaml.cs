using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.ManifestDialog
{
  /// <summary>
  ///   Interaction logic for SettingsDialog.xaml
  /// </summary>
  public partial class SettingsDialog : Window
  {
    public SettingsDialog()
    {
      InitializeComponent();
      RootDirectoryPath.Text = GetCurrentRootDirectory();
    }

    public string RootDirectory => RootDirectoryPath.Text;

    private string GetCurrentRootDirectory()
    {
      var rootDirectory = TemplatesRootDirectoryPathRepository.Get();
      if (!string.IsNullOrEmpty(rootDirectory))
        return rootDirectory;
      MessageBox.Show("Please select where you want your Helix module and solution templates stored.\n\nIf you already selected a location please restart Visual Studio as administrator.", "Welcome", MessageBoxButton.OK);
      return TemplatesRootDirectoryPathRepository.DefaultRootPath;
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
      if (!TemplatesRootDirectoryPathRepository.Set(selectedRootpath))
      {
        MessageBox.Show("Could not save selected root path. Please ensure that you are running this Visual Studio instance as administrator.", "Error", MessageBoxButton.OK);
        return;
      }
      if (!RootHasTemplateManifests(selectedRootpath))
      {
        var confirmResult = MessageBox.Show("Selected root does not contain templates.\n\nDo you want to install the default templates (recommended)?", "Confirm", MessageBoxButton.YesNo);
        if (confirmResult != MessageBoxResult.Yes)
          return;
        UnzipTemplatesArchive(selectedRootpath);
      }
      DialogResult = true;
      Close();
    }

    private void UnzipTemplatesArchive(string selectedRootpath)
    {
      var tempFile = Path.GetTempFileName();
      File.WriteAllBytes(tempFile, Properties.Resources.StandardTemplates);
      ZipFile.ExtractToDirectory(tempFile, selectedRootpath);
    }

    private bool RootHasTemplateManifests(string selectedRootpath)
    {
      return new DirectoryInfo(selectedRootpath).EnumerateFiles("template.manifest.xml", SearchOption.AllDirectories).Any();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }

    private void UnpackTemplates_Clicked(object sender, RoutedEventArgs e)
    {
      var rootDirectory = TemplatesRootDirectoryPathRepository.Get();
      if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
        MessageBox.Show(this, "You need to select a valid root directory.", "Error", MessageBoxButton.OK);
      if (Directory.EnumerateDirectories(rootDirectory).Any())
      {
        var overwriteConfirmResult = MessageBox.Show("This will overwrite changes made to built-in templates in this root folder.\nAre you sure you want to continue?\n\nNote: Always make your modifications changes in copies and never directly in the built-in example templates.", "Confirm", MessageBoxButton.YesNo);
        if (overwriteConfirmResult != MessageBoxResult.Yes)
        {
          return;
        }
        DeleteExistingBuiltInTemplates(rootDirectory);
      }
      UnzipTemplatesArchive(rootDirectory);
    }

    private void DeleteExistingBuiltInTemplates(string rootDirectory)
    {
      using (var zipArchive = new ZipArchive(new MemoryStream(Properties.Resources.StandardTemplates), ZipArchiveMode.Read))
      {
        var directories = zipArchive.Entries.Select(e => e.FullName.Split('\\')[0]).Distinct();
        foreach (var directory in directories)
        {
          var dieDirectory = Path.GetFullPath(Path.Combine(rootDirectory, directory));
          if (!Directory.Exists(dieDirectory))
            continue;
          Directory.Delete(dieDirectory, true);
        }
      }
    }
  }
}