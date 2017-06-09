using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public class ValidateNamespaceToken : ValidateTokenByRegEx
  {
    protected override Regex Expression => new Regex(@"^(?:[A-Z][\w0-9\._]+)+[\w0-9_]$", RegexOptions.Compiled);
    protected override string Message => "{0} is not a valid namespace";
  }
}