using System.Collections.Generic;
using System.Linq;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class ScopedTemplateSettings
  {
    protected IDictionary<string, string> ConfigurationParameters;

    public ScopedTemplateSettings()
    {
      ConfigurationParameters = new Dictionary<string, string>();
    }

    public ScopedTemplateSettings(IEnumerable<string> configurationParams)
    {
      ConfigurationParameters = configurationParams.ToDictionary(line => line.Split('=')[0].ToLowerInvariant(),
        line => line.Split('=')[1].ToLowerInvariant());
    }

    public string GetSetting(string key, string defaultValue = "")
    {
      return ConfigurationParameters.ContainsKey(key)
        ? ConfigurationParameters[key]
        : defaultValue;
    }

    public bool GetBooleanSetting(string key, bool defaultValue = false)
    {
      return ConfigurationParameters.ContainsKey(key)
             && bool.TryParse(ConfigurationParameters[key], out var b)
             && b || defaultValue;
    }

    public void Set(string key, string value)
    {
      ConfigurationParameters[key] = value;
    }

    public void Set(string key, bool value)
    {
      Set(key, value.ToString());
    }

    public override string ToString()
    {
      return string.Join("\n", ConfigurationParameters.Select(kv => $"{kv.Key}={kv.Value.ToLowerInvariant()}"));
    }
  }
}