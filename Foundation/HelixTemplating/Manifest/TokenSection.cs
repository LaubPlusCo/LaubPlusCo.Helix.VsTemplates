using System.Collections.Generic;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public class TokenSection : ITokenSection
  {
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public IList<ITokenDescription> Tokens { get; set; }
  }
}