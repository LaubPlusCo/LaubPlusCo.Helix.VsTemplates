using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class ReplaceTokensInFilesService
  {
    protected const decimal MaxWaitHandleThreads = 63;
    protected readonly string[] FilePaths;
    protected readonly IDictionary<string, string> ReplacementTokens;
    protected readonly ReplaceTokensService ReplaceTokensService;

    public ReplaceTokensInFilesService(string[] filePaths, IDictionary<string, string> replacementTokens)
    {
      FilePaths = filePaths;
      ReplacementTokens = replacementTokens;
      ReplaceTokensService = new ReplaceTokensService(replacementTokens);
    }

    public void Replace()
    {
      var replacementThread = new Thread(RunReplacements);
      replacementThread.Start();
    }

    public void RunReplacements()
    {
      if (FilePaths.Length <= 0)
        throw new ArgumentException("No files to replace tokens in");
      var results = new List<FileTokenReplaceResult>();
      var doneEvents = new List<ManualResetEvent>();

      for (var i = 0; i < FilePaths.Length; i++)
      {
        var doneEvent = new ManualResetEvent(false);
        var fileTokenReplacer = new FileTokenReplace(FilePaths[i], ReplaceTokensService,
          doneEvent);
        var result = new FileTokenReplaceResult();
        results.Add(result);
        ThreadPool.QueueUserWorkItem(fileTokenReplacer.ReplaceTokens, result);
        doneEvents.Add(doneEvent);
        if (doneEvents.Count < MaxWaitHandleThreads && i != FilePaths.Length - 1)
          continue;
        WaitHandle.WaitAll(doneEvents.ToArray());
        doneEvents.Clear();
      }

      WriteTraceService.WriteToTrace("File content token replacement results:", "\nInfo",
        results.Select(GetResultAsString).ToArray());
    }

    private string GetResultAsString(FileTokenReplaceResult result)
    {
      var fileInfo = $"[{result.Status}] \"{result.FilePath}\"";
      if (result.Status == FileTokenReplacementStatus.Failed 
          || result.Status == FileTokenReplacementStatus.Skipped)
        return fileInfo;
      if (result.ReplacementCounter == null || !result.ReplacementCounter.Any())
        return string.Concat(fileInfo, " - no tokens found to replace");
      var replacements = string.Join("\n\t\t", result.ReplacementCounter.Select(c => $"{c.Key}: {c.Value} occurrences replaced "));
      return string.Concat(fileInfo, "\n\t\t", replacements);
    }
  }
}