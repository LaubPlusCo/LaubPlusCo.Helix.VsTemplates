using System;
using System.IO;
using System.Net;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class DownloadFileService
  {
    protected const int BufferSize = 16 * 1024;
    public string Message { get; protected set; }

    public bool TryDownloadFromUrl(string url, out string downloadedFilePath)
    {
      downloadedFilePath = string.Empty;
      if (string.IsNullOrEmpty(url))
        return false;

      try
      {
        var request = (HttpWebRequest) WebRequest.Create(url);
        request.Method = WebRequestMethods.Http.Get;
        downloadedFilePath = GetDestinationFilePath(url);

        if (File.Exists(downloadedFilePath))
          File.Delete(downloadedFilePath);

        using (var fileStream = new FileStream(downloadedFilePath, FileMode.CreateNew))
        {
          using (var responseStream = request.GetResponse().GetResponseStream())
          {
            if (responseStream == null)
            {
              fileStream.Close();
              return false;
            }

            var buffer = new byte[BufferSize];
            int bytesRead;
            do
            {
              bytesRead = responseStream.Read(buffer, 0, BufferSize);
              fileStream.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);
          }
        }
      }
      catch (Exception e)
      {
        Message = e.Message;
        return false;
      }

      return true;
    }

    private string GetDestinationFilePath(string url)
    {
      var fileName = Path.GetFileName(url);
      if (string.IsNullOrEmpty(fileName))
        fileName = "download.zip";
      return Path.Combine(FileStorageService.Instance.TemporaryDirectory.FullName, fileName);
    }
  }
}