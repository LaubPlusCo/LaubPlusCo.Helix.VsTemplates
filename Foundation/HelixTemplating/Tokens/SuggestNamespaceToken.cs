using System;
using System.Collections.Generic;
using System.Linq;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public class SuggestNamespaceToken : ISuggestToken
  {
    public SuggestNamespaceToken()
    {
      DependentOnKeys = new List<string>() {"$moduleName$"};
    }
    public IList<string> DependentOnKeys { get; set; }
    public bool TriggerOnTextChange { get; set; }
    public string Suggest(string currentValue, string key, string value)
    {
      if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        return currentValue;
      if (currentValue.EndsWith("." + value))
        return currentValue;
      if (value.Contains("."))
        value = value.Substring(value.IndexOf(".", StringComparison.InvariantCulture) + 1);
      var namespaceParts = currentValue.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
      if (namespaceParts.Length == 1)
        return namespaceParts[0] + "." + value;
      namespaceParts[namespaceParts.Length - 1] = value;
      return string.Join(".", namespaceParts);
    }
  }
}