using System.Linq;
using System.Windows.Controls;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  public partial class TokenSelectionInput : TokenInputControl
  {
    public TokenSelectionInput(TokenDescription tokenDescription)
    {
      InitializeComponent();
      TokenDescription = tokenDescription;
      TokenComboInput.SelectionChanged += InputChangedEventHandler;
      TokenComboInput.ItemsSource = TokenDescription.SelectionOptions;
      TokenComboInput.DisplayMemberPath = "Value";
      TokenComboInput.SelectedValuePath = "Key";
    }

    public override string TokenValue
    {
      get => (string) TokenComboInput?.SelectedValue;
      set => TokenComboInput.SelectedIndex = TokenDescription.SelectionOptions.ToList().IndexOf(TokenDescription.SelectionOptions.FirstOrDefault(t => t.Key == value));
    }

    public override Label DisplayNameLabel => TokenDisplayNameLabel;
    public override Control InputControl => TokenComboInput;
  }
}