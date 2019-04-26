using System.Collections.Generic;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class FileTokenReplaceResult
  {
    public FileTokenReplacementStatus Status { get; set; }
    public string FilePath { get; set; }
    public IDictionary<string, int> ReplacementCounter { get; set; }
  }

  public enum FileTokenReplacementStatus
  {
    Failed,
    Success,
    Skipped
  }
}