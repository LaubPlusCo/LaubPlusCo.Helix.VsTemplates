using System;
using System.Collections.Generic;
using System.Linq;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public class SuggestNamespaceToken : ISuggestToken
  {
    private const string ModuleNameKey = "$moduleName$";
    private const string ModuleNameKeyLowerCaseFirst = "$moduleNameLowerCaseFirst$";
    private const string LayerNameKey = "$layerName$";
    public SuggestNamespaceToken()
    {
      DependentOnKeys = new List<string>() { ModuleNameKey, LayerNameKey, ModuleNameKeyLowerCaseFirst };
    }

    public IList<string> DependentOnKeys { get; set; }
    public bool TriggerOnTextChange { get; set; }
    public string Suggest(string currentValue, string key, string value)
    {
      if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        return currentValue;

      if (key.Equals(ModuleNameKey))
        return SuggestByModuleName(currentValue, value);

      if (key.Equals(LayerNameKey))
        return SuggestByLayerName(currentValue, value);

      if (key.Equals(ModuleNameKeyLowerCaseFirst))
        return SuggestByModuleName(currentValue, value, true);

      return value;
    }

    private string SuggestByModuleName(string currentValue, string value, bool lowerCaseFirst = false)
    {
      if (lowerCaseFirst)
        currentValue = currentValue.First().ToString().ToUpper() + currentValue.Substring(1);

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