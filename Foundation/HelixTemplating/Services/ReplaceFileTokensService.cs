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
      var threadBatches = Math.Ceiling(FilePaths.Length / MaxWaitHandleThreads);
      var results = new List<FileTokenReplaceResult>();

      for (var i = 0; i < threadBatches; i++)
      {
        var remainder = FilePaths.Length > MaxWaitHandleThreads
          ? i * MaxWaitHandleThreads < FilePaths.Length ? MaxWaitHandleThreads : FilePaths.Length % MaxWaitHandleThreads
          : FilePaths.Length;

        var doneEvents = new WaitHandle[FilePaths.Length];
        var fileTokenReplacers = new List<FileTokenReplace>();

        for (var j = 0; j < remainder; j++)
        {
          var filePathIndex = (int) MaxWaitHandleThreads * i + j;
          doneEvents[filePathIndex] = new ManualResetEvent(false);

          var fileTokenReplacer = new FileTokenReplace(FilePaths[filePathIndex], ReplaceTokensService,
            (ManualResetEvent) doneEvents[filePathIndex]);
          fileTokenReplacers.Add(fileTokenReplacer);
          ThreadPool.QueueUserWorkItem(fileTokenReplacer.ReplaceTokens, filePathIndex);
        }

        WaitHandle.WaitAll(doneEvents);
        results.AddRange(fileTokenReplacers.Select(ftr => new FileTokenReplaceResult
        {
          FilePath = ftr.FilePath,
          Success = ftr.Success,
          ReplacementCounter = ftr.ReplacementCounter
        }));
      }

      //TODO: Log replacement results.!
    }
  }
}