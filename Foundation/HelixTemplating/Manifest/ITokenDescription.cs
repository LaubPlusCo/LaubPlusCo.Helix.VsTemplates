using System.Collections.Generic;
using LaubPlusCo.Foundation.HelixTemplating.Tokens;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public interface ITokenDescription
  {
    string Key { get; set; }
    string DisplayName { get; set; }
    string HelpText { get; set; }
    IValidateToken Validator { get; set; }
    ISuggestToken Suggestor { get; set; }
    bool IsRequired { get; set; }
    string Default { get; set; }
    TokenInputForm InputType { get; set; }
    IList<KeyValuePair<string, string>> SelectionOptions { get; set; }
  }
}