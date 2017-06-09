using System.Text.RegularExpressions;

namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public abstract class ValidateTokenByRegEx : IValidateToken
  {
    protected abstract Regex Expression { get; }
    protected abstract string Message { get; }

    public IValidateTokenResult Validate(string token)
    {
      return Expression.IsMatch(token)
        ? ValidateTokenResult.Success
        : new ValidateTokenResult {IsValid = false, Message = string.Format(Message, token)};
    }
  }
}