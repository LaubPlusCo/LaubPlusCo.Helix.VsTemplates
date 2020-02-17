using System.Collections.Generic;
using LaubPlusCo.Foundation.HelixTemplating.Tokens;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public class TokenDescription : ITokenDescription
  {
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public string HelpText { get; set; }
    public IValidateToken Validator { get; set; }
    public ISuggestToken Suggestor { get; set; }
    public bool IsRequired { get; set; }
    public string Default { get; set; }
    public TokenInputForm InputType { get; set; }
    public IList<KeyValuePair<string,string>> SelectionOptions { get; set; }
  }
}