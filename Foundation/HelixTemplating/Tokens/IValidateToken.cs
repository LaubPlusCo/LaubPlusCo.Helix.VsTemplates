namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public interface IValidateToken
  {
    IValidateTokenResult Validate(string token);
  }
}