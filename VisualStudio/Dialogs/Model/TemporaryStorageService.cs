using System.IO;
using System.Security.AccessControl;
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
      var tempDirectory = new DirectoryInfo(TempFolderPath);
      if (tempDirectory.Exists)
        tempDirectory.Delete(true);
      tempDirectory.Create();
      return tempDirectory;
    }

    public void CleanTempFolder()
    {
      TemporaryDirectory = GetTemporaryDirectory();
    }

    public void Delete(string path)
    {
      var directoryInfo = new DirectoryInfo(path);
      if (!directoryInfo.Exists) return;
      directoryInfo.Delete(true);
    }

    public void RemoveTempFolder()
    {
      Delete(TempFolderPath);
    }
  }
}