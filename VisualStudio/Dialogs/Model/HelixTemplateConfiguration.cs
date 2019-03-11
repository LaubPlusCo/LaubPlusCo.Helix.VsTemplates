using System.Collections.Generic;
using System.Linq;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class HelixTemplateConfiguration
  {
    protected IDictionary<string, string> ConfigurationParameters;
    protected const string ModuleTemplatesFolderKey = "templates.modules.folder";

    public HelixTemplateConfiguration()
    {
      ConfigurationParameters = new Dictionary<string, string>();
    }

    public HelixTemplateConfiguration(IEnumerable<string> configurationParams)
    {
      ConfigurationParameters = configurationParams.ToDictionary(line => line.Split('=')[0].ToLowerInvariant(), line => line.Split('=')[1].ToLowerInvariant());
    }

    public string ModuleTemplatesFolder
    {
      get => ConfigurationParameters.ContainsKey(ModuleTemplatesFolderKey)
        ? ConfigurationParameters[ModuleTemplatesFolderKey]
        : string.Empty;
      set => ConfigurationParameters[ModuleTemplatesFolderKey] = value;
    }

    public override string ToString()
    {
      return string.Join("\n", ConfigurationParameters.Select(kv => $"{kv.Key}={kv.Value}"));
    }
  }
}