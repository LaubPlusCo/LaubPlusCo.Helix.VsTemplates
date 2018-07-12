using System;
using System.Collections.Generic;
using System.Linq;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public class SuggestNamespaceToken : ISuggestToken
  {
    private const string ModuleNameKey = "$moduleName$";
    private const string LayerNameKey = "$layerName$";
    public SuggestNamespaceToken()
    {
      DependentOnKeys = new List<string>() { ModuleNameKey, LayerNameKey };
    }

    public IList<string> DependentOnKeys { get; set; }
    public bool TriggerOnTextChange { get; set; }
    public string Suggest(string currentValue, string key, string value)
    {
      if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        return currentValue;
      return key.Equals(ModuleNameKey) ? SuggestByModuleName(currentValue, value) : (key.Equals(LayerNameKey) ? SuggestByLayerName(currentValue, value) : value);
    }

    private string SuggestByModuleName(string currentValue, string value)
    {
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

    private string SuggestByLayerName(string currentValue, string value)
    {
      if (currentValue.Contains(value + "."))
        return currentValue;
      if (value.Contains("."))
        value = value.Substring(value.IndexOf(".", StringComparison.InvariantCulture) + 1);
      var namespaceParts = currentValue.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
      if (namespaceParts.Length == 1)
        return value + "." + namespaceParts[0];
      namespaceParts[namespaceParts.Length - 2] = value;
      return string.Join(".", namespaceParts);
    }
  }
}