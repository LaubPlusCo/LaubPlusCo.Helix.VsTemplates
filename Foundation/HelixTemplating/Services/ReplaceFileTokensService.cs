using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class ReplaceTokensInFilesService
  {
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
      var doneEvents = new WaitHandle[FilePaths.Length];
      var fileTokenReplacers = new List<FileTokenReplace>();

      for (var i = 0; i < FilePaths.Length; i++)
      {
        doneEvents[i] = new ManualResetEvent(false);
        var fileTokenReplacer = new FileTokenReplace(FilePaths[i], ReplaceTokensService, (ManualResetEvent)doneEvents[i]);
        fileTokenReplacers.Add(fileTokenReplacer);
        ThreadPool.QueueUserWorkItem(fileTokenReplacer.ReplaceTokens, i);
      }
      WaitHandle.WaitAll(doneEvents);
      var results = fileTokenReplacers.Select(ftr => new FileTokenReplaceResult
      {
        FilePath = ftr.FilePath,
        Success = ftr.Success,
        ReplacementCounter = ftr.ReplacementCounter
      }).ToArray();

      //TODO: Log replacement results.!

    }
  }
}