using System.Text.RegularExpressions;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public class ValidateVersionNumberToken : ValidateTokenByRegEx
  {
    protected override Regex Expression => new Regex(@"^(\d+\.)?(\d+\.)?(\*|\d+)$", RegexOptions.Compiled);
    protected override string Message => "{0} is not a valid version number.";
  }
}