using System.Collections.Generic;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class FileTokenReplaceResult
  {
    public bool Success { get; set; }
    public string FilePath { get; set; }
    public IDictionary<string, int> ReplacementCounter { get; set; }
  }
}