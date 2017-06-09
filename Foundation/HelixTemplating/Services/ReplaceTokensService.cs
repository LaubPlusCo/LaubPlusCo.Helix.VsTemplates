using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class ReplaceTokensService
  {
    protected readonly IDictionary<string, string> ReplacementTokens;

    public ReplaceTokensService(IDictionary<string, string> replacementTokens)
    {
      ReplacementTokens = new Dictionary<string, string>(replacementTokens, StringComparer.InvariantCultureIgnoreCase);
    }

    public virtual string Replace(string input)
    {
      return Replace(input, out IDictionary<string, int> ignoreCount);
    }

    public string Replace(string input, out IDictionary<string, int> replacementsCount)
    {
      return EscapeBackSlashes(DoReplacement(input, out replacementsCount));
    }

    protected virtual string EscapeBackSlashes(string input)
    {
      return Regex.Replace(input, @"!!.+!!", match =>
      {
        var value = match.Groups[0].Value;
        return value.Substring(2, value.Length - 4).Replace(@"\", @"\\");
      });
    }

    protected virtual string DoReplacement(string input, out IDictionary<string, int> replacementsCount)
    {
      var internalCount = new Dictionary<string, int>();
      var output = Regex.Replace(input, @"\$\w+\$", delegate(Match match)
      {
        var key = match.Groups[0].Value.ToLowerInvariant();
        if (!ReplacementTokens.TryGetValue(key, out string replacement))
          return key;
        if (internalCount.ContainsKey(key))
        {
          internalCount[key] += 1;
        }
        else
        {
          internalCount.Add(key, match.Groups.Count);
        }

        return replacement;
      }, RegexOptions.IgnoreCase);
      replacementsCount = internalCount;
      return output;
    }
  }
}