using System.Collections.Generic;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public interface ISuggestToken
  {
    IList<string> DependentOnKeys { get; set; }

    bool TriggerOnTextChange { get; set; }

    string Suggest(string tokenValue, string key, string value);
  }
}