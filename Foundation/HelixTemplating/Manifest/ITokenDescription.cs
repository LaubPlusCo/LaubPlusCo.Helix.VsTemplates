using LaubPlusCo.Foundation.HelixTemplating.Tokens;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public class TokenDescription
  {
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public IValidateToken Validator { get; set; }
    public ISuggestToken Suggestor { get; set; }
    public bool IsRequired { get; set; }
    public bool IsFolder { get; set; }
    public string Default { get; set; }
  }
}