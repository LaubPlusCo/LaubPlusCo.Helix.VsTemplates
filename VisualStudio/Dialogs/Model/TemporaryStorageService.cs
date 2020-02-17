using System.IO;
using System.Threading;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class FileStorageService
  {
    private static FileStorageService _instance;
    public DirectoryInfo TemporaryDirectory;

    public FileStorageService()
    {
      TemporaryDirectory = GetTemporaryDirectory();
    }

    public static FileStorageService Instance => _instance ?? (_instance = new FileStorageService());

    protected DirectoryInfo GetTemporaryDirectory()
    {
      var tempPath = Path.Combine(Path.GetTempPath(), ".vst");
      if (Directory.Exists(tempPath))
        Delete(tempPath);
      return Directory.CreateDirectory(tempPath);
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
  }
}