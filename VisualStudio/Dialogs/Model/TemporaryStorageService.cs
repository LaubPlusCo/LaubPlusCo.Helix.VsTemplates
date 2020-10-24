using System.IO;
using System.Threading;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class FileStorageService
  {
    private static FileStorageService _instance;
    protected readonly string TempFolderPath;
    public DirectoryInfo TemporaryDirectory;

    public FileStorageService()
    {
      TempFolderPath = AppScopeSettings.Current.TempFolderPath;
      TemporaryDirectory = GetTemporaryDirectory();
    }

    public static FileStorageService Instance => _instance ?? (_instance = new FileStorageService());

    protected DirectoryInfo GetTemporaryDirectory()
    {
      if (Directory.Exists(TempFolderPath))
        Delete(TempFolderPath);
      return Directory.CreateDirectory(TempFolderPath);
    }

    public void CleanTempFolder()
    {
      TemporaryDirectory = GetTemporaryDirectory();
    }

    public void Delete(string path)
    {
      if (!Directory.Exists(path)) return;
      var allFiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
      foreach (var file in allFiles) File.Delete(file);
      Directory.Delete(path, true);
      Thread.Sleep(500);
    }

    public void RemoveTempFolder()
    {
      Delete(TempFolderPath);
    }
  }
}