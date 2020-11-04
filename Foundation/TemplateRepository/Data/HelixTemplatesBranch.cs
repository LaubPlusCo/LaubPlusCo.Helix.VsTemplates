using Newtonsoft.Json;

namespace LaubPlusCo.Foundation.Helix.TemplateRepository.Data
{

  public class HelixTemplatesBranch
  {
    public string DisplayName => BranchName.Contains("/") ? BranchName.Split('/')[1] : (BranchName.Equals("master") ? "Latest" : BranchName);
    public string Type => BranchName.Contains("/") ? BranchName.Split('/')[0] : "Main";
    
    [JsonProperty("name")]
    public string BranchName { get; set; }
  }
}
