using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Tokens;
using TextBox = System.Windows.Controls.TextBox;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.ManifestDialog
{
  /// <summary>
  ///   Interaction logic for TokenInput.xaml
  /// </summary>
  public partial class TokenInput
  {
    private TokenDescription _tokenDescription;

    public TokenInput()
    {
      InitializeComponent();
    }

    private TextBox ActiveInputTextBox => _tokenDescription.IsFolder ? TokenValueAsPathInputBox : TokenValueInputBox;
    public TokenInput[] DependendTokenInputs;

    public TokenDescription TokenDescription
    {
      protected get => _tokenDescription;
      set
      {
        _tokenDescription = value;
      }
    }

    public string TokenValue
    {
      get => ActiveInputTextBox.Text;
      set => ActiveInputTextBox.Text = value;
    }

    protected IValidateToken Validator => TokenDescription.Validator;
    public ISuggestToken Suggestor => TokenDescription.Suggestor;
    public string TokenKey => TokenDescription.Key;

    public void Initialize()
    {
      if (!string.IsNullOrEmpty(_tokenDescription.Default))
        TokenValue = _tokenDescription.Default;
      TokenDisplayNameLabel.Content = _tokenDescription.DisplayName;

      if (DependendTokenInputs != null && Suggestor != null)
      {
        SubscribeOnDependentTokens();
      }
      ActiveInputTextBox.TextChanged += TextChanged;

      if (_tokenDescription.IsFolder)
      {
        TokenValueInputBox.Visibility = Visibility.Hidden;
        TokenValueAsPathInputBox.Visibility = Visibility.Visible;
        FolderBrowseInput.Visibility = Visibility.Visible;
        return;
      }

      TokenValueInputBox.Visibility = Visibility.Visible;
      FolderBrowseInput.Visibility = Visibility.Hidden;
    }

    private void TextChanged(object sender, TextChangedEventArgs e)
    {
      OnValueChanged(new TokenValueChangedArgs { Value = TokenValueInputBox.Text, Key = TokenDescription.Key });
      if (Validator == null)
        return;
      ActiveInputTextBox.Background = Validator.Validate(ActiveInputTextBox.Text).IsValid ? Brushes.LightGreen : Brushes.Khaki;
    }

    private void SubscribeOnDependentTokens()
    {
      foreach (var dependenTokenInput in DependendTokenInputs)
      {
        dependenTokenInput.ValueChanged += DependentValueChanged;
      }
    }

    private void DependentValueChanged(object sender, TokenValueChangedArgs e)
    {
      TokenValue = Suggestor.Suggest(TokenValue, e.Key, e.Value);
    }

    protected virtual void OnValueChanged(TokenValueChangedArgs e)
    {
      ValueChanged?.Invoke(this, e);
    }

    public event EventHandler<TokenValueChangedArgs> ValueChanged;

    public IValidateTokenResult Validate()
    {
      var result = PerformValidatation();
      if (result.IsValid)
        return result;
      SetInvalidValueStyle();
      return result;
    }

    private void SetInvalidValueStyle()
    {
      ActiveInputTextBox.Background = Brushes.OrangeRed;
    }

    private IValidateTokenResult PerformValidatation()
    {
      if (_tokenDescription.IsRequired && string.IsNullOrWhiteSpace(TokenValue))
        return new ValidateTokenResult { IsValid = false, Message = $"{_tokenDescription.DisplayName} is required." };
      if (Validator == null)
        return ValidateTokenResult.Success;
      return Validator == null ? ValidateTokenResult.Success : Validator.Validate(TokenValue);
    }

    private void BrowseFolders(object sender, RoutedEventArgs e)
    {
      using (var browseDialog = new FolderBrowserDialog())
      {
        browseDialog.SelectedPath = GetSelectedPath();
        browseDialog.ShowNewFolderButton = true;
        var result = browseDialog.ShowDialog();
        if (result == DialogResult.OK)
          TokenValue = browseDialog.SelectedPath;
      }
    }

    private string GetSelectedPath()
    {
      return string.IsNullOrEmpty(TokenValue) || !Path.IsPathRooted(TokenValue) ? @"c:\" : TokenValue;
    }
  }
}