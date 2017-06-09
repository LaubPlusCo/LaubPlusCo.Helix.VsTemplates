using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class FileTokenReplace
  {

    private readonly ManualResetEvent _doneEvent;
    private readonly ReplaceTokensService _tokensService;
    public readonly string FilePath;

    public FileTokenReplace(string filePath, ReplaceTokensService tokensService, ManualResetEvent done)
    {
      FilePath = filePath;
      _tokensService = tokensService;
      _doneEvent = done;
    }

    public bool Success { get; protected set; }
    public IDictionary<string, int> ReplacementCounter { get; set; }

    public virtual void ReplaceTokens(object threadContext)
    {
      if (!File.Exists(FilePath) || !CanModify(FilePath))
      {
        Success = false;
        _doneEvent.Set();
        return;
      }
      if (FileTypeIsIgnored(FilePath))
      {
        Success = true;
        _doneEvent.Set();
        return;
      }
      var fileContent = File.ReadAllText(FilePath);
      var parsedContent = _tokensService.Replace(fileContent, out IDictionary<string, int> replacementsCount);
      File.WriteAllText(FilePath, parsedContent);
      ReplacementCounter = replacementsCount;
      _doneEvent.Set();
    }

    protected virtual bool FileTypeIsIgnored(string filePath)
    {
      var fileExtension = Path.GetExtension(filePath);
      return BinaryFiles.Extensions.Any(i => i.Equals(fileExtension));
    }

    protected virtual bool CanModify(string filePath)
    {
      if ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        return false;
      var file = new FileInfo(filePath);
      try
      {
        using (file.OpenWrite())
        {
          return true;
        }
      }
      catch
      {
        return false;
      }
    }
  }
}