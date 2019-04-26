using System.Collections.Generic;
using System.Linq;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class SolutionScopeTemplateSettings
  {
    protected IDictionary<string, string> ConfigurationParameters;
    protected const string ModuleTemplatesFolderKey = "templates.modules.folder";
    protected const string SkipCreateFolderDialogKey = "templates.modules.skipcreatedialog";

    public SolutionScopeTemplateSettings()
    {
      ConfigurationParameters = new Dictionary<string, string>();
    }

    public SolutionScopeTemplateSettings(IEnumerable<string> configurationParams)
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

    public bool SkipCreateFolderDialog
    {
      get => ConfigurationParameters.ContainsKey(SkipCreateFolderDialogKey) 
             && bool.TryParse(ConfigurationParameters[SkipCreateFolderDialogKey], out var b) 
             && b;
      set => ConfigurationParameters[SkipCreateFolderDialogKey] = value.ToString();
    }

    public override string ToString()
    {
      return string.Join("\n", ConfigurationParameters.Select(kv => $"{kv.Key}={kv.Value.ToLowerInvariant()}"));
    }
  }
}