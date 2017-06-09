namespace LaubPlusCo.Foundation.HelixTemplating.Tokens
{
  public class ValidateTokenResult : IValidateTokenResult
  {
    public bool IsValid { get; set; }
    public string Message { get; set; }
    public static ValidateTokenResult Success => new ValidateTokenResult { IsValid = true };
  }
}