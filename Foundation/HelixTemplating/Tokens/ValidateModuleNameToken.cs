using System.Text.RegularExpressions;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public class ValidateModuleNameToken : ValidateTokenByRegEx
  {
    protected override Regex Expression => new Regex(@"^(?:[A-Z][\w0-9]+)$", RegexOptions.Compiled);
    protected override string Message => "The module name {0} is invalid - Should start with a capital letter and only contain characters a-z|A-Z|0-9";
  }
}