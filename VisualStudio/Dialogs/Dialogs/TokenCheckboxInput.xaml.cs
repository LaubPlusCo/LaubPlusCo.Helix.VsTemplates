using System.Windows.Controls;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for TokenCheckboxInput.xaml
  /// </summary>
  public partial class TokenCheckboxInput : TokenInputControl
  {
    public TokenCheckboxInput(ITokenDescription tokenDescription)
    {
      InitializeComponent();
      TokenDescription = tokenDescription;
      this.SetVisualStudioThemeStyles();
    }

    public override string TokenValue
    {
      get => TokenInputCheckbox.IsChecked.HasValue && TokenInputCheckbox.IsChecked.Value
        ? bool.TrueString
        : bool.FalseString;
      set => TokenInputCheckbox.IsChecked = bool.TryParse(value, out var val) && val;
    }

    public override Label DisplayNameLabel => TokenDisplayNameLabel;
    public override Control InputControl => TokenInputCheckbox;
    public override TextBlock HelpTextBlock => TokenHelpTextBlock;
  }
}