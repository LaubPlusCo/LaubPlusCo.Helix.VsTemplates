namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public interface IValidateTokenResult
  {
    bool IsValid { get; set; }
    string Message { get; set; }
  }
}