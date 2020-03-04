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

    public virtual void ReplaceTokens(object scopeResult)
    {
      var result = (FileTokenReplaceResult) scopeResult;
      result.FilePath = FilePath;

      if (!File.Exists(FilePath) || !CanModify(FilePath))
      {
        result.Status = FileTokenReplacementStatus.Failed;
        _doneEvent.Set();
        return;
      }
      if (FileTypeIsIgnored(FilePath))
      {
        result.Status = FileTokenReplacementStatus.Skipped;
        _doneEvent.Set();
        return;
      }

      var fileContent = File.ReadAllText(FilePath);
      var parsedContent = _tokensService.Replace(fileContent, out var replacementsCounter);
      File.WriteAllText(FilePath, parsedContent);

      result.ReplacementCounter = replacementsCounter;
      result.Status = FileTokenReplacementStatus.Success;

      _doneEvent.Set();
    }

    protected virtual bool FileTypeIsIgnored(string filePath)
    {
      var fileExtension = Path.GetExtension(filePath)?.TrimStart('.');
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