using System;
using System.Collections.Generic;
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
      var fileTokenReplacers = new List<FileTokenReplace>();
      var doneEvents = new List<ManualResetEvent>();

      for (var i = 0; i < FilePaths.Length; i++)
      {
        var doneEvent = new ManualResetEvent(false);
        var fileTokenReplacer = new FileTokenReplace(FilePaths[i], ReplaceTokensService,
          doneEvent);
        fileTokenReplacers.Add(fileTokenReplacer);
        ThreadPool.QueueUserWorkItem(fileTokenReplacer.ReplaceTokens, i);
        doneEvents.Add(doneEvent);
        if (doneEvents.Count < MaxWaitHandleThreads || i == FilePaths.Length - 1)
          continue;
        WaitHandle.WaitAll(doneEvents.ToArray());
        doneEvents.Clear();
      }

      results.AddRange(fileTokenReplacers.Select(ftr => new FileTokenReplaceResult
      {
        FilePath = ftr.FilePath,
        Success = ftr.Success,
        ReplacementCounter = ftr.ReplacementCounter
      }));
      //TODO: Log replacement results.!
    }
  }
}