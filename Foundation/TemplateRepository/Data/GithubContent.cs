using Newtonsoft.Json;

namespace LaubPlusCo.Foundation.Helix.TemplateRepository.Data
{
  public class GithubContent
  {
    [JsonProperty("name")]
    public string Name { get; set; }
  }
}
