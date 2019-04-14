using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Tokens;
using Microsoft.VisualStudio.Shell;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  public abstract class TokenInputControl : UserControl
  {
    protected TokenInputControl()
    {
    }

    public TokenInputControl[] DependendTokenInputs;
    public TokenDescription TokenDescription { get; set; }
    public abstract string TokenValue { get; set; }
    protected IValidateToken Validator => TokenDescription.Validator;
    public ISuggestToken Suggestor => TokenDescription.Suggestor;
    public string TokenKey => TokenDescription.Key;
    public abstract Label DisplayNameLabel { get; }
    public abstract TextBlock HelpTextBlock { get; }
    public abstract Control InputControl { get; }

    public event EventHandler InputChangedEvent;
    public event EventHandler<TokenValueChangedArgs> ValueChanged;

    public virtual void Initialize()
    {
      if (!string.IsNullOrEmpty(TokenDescription.Default))
        TokenValue = TokenDescription.Default;

      DisplayNameLabel.Content = TokenDescription.DisplayName;
      SetHelpTextLabel(TokenDescription.HelpText);

      if (DependendTokenInputs != null && Suggestor != null)
        SubscribeOnDependentTokens();
      InputChangedEvent += TokenValueChanged;
    }

    protected virtual void SetHelpTextLabel(string helpText)
    {
      if (string.IsNullOrEmpty(TokenDescription.HelpText))
      {
        HelpTextBlock.Visibility = Visibility.Collapsed;
        return;
      }
      HelpTextBlock.Text = TokenDescription.HelpText;
    }

    protected virtual void TokenValueChanged(object sender, EventArgs e)
    {
      OnValueChanged((TokenValueChangedArgs)e);
      if (Validator == null)
        return;
      InputControl.Background = Validator.Validate(TokenValue).IsValid ? Brushes.LightGreen : Brushes.Khaki;
    }

    protected virtual void SubscribeOnDependentTokens()
    {
      foreach (var dependenTokenInput in DependendTokenInputs)
        dependenTokenInput.ValueChanged += DependentValueChanged;
    }

    protected virtual void DependentValueChanged(object sender, TokenValueChangedArgs e)
    {
      TokenValue = Suggestor.Suggest(TokenValue, e.Key, e.Value);
    }

    protected virtual void OnValueChanged(TokenValueChangedArgs e)
    {
      ValueChanged?.Invoke(this, e);
    }

    public IValidateTokenResult Validate()
    {
      var result = PerformValidatation();
      if (result.IsValid)
        return result;
      SetInvalidValueStyle();
      return result;
    }

    protected virtual void SetInvalidValueStyle()
    {
      InputControl.Background = Brushes.OrangeRed;
    }

    protected virtual IValidateTokenResult PerformValidatation()
    {
      if (TokenDescription.IsRequired && string.IsNullOrWhiteSpace(TokenValue))
        return new ValidateTokenResult { IsValid = false, Message = $"{TokenDescription.DisplayName} is required." };
      if (Validator == null)
        return ValidateTokenResult.Success;
      return Validator == null ? ValidateTokenResult.Success : Validator.Validate(TokenValue);
    }

    protected virtual void OnInputChangedEvent()
    {
      InputChangedEvent?.Invoke(this, new TokenValueChangedArgs { Key = TokenKey, Value = TokenValue });
    }

    protected virtual void InputChangedEventHandler(object sender, EventArgs e)
    {
      OnInputChangedEvent();
    }
  }
}