using System.Collections.Generic;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public interface ITokenSection
  {
    string DisplayName { get; set; }
    string Description { get; set; }
    IList<ITokenDescription> Tokens { get; set; }
  }
}