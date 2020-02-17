using System.Windows.Controls;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for TokenTextInput.xaml
  /// </summary>
  public partial class TokenTextInput : TokenInputControl
  {
    public TokenTextInput(ITokenDescription tokenDescription)
    {
      InitializeComponent();
      TokenValueInputBox.TextChanged += InputChangedEventHandler;
      TokenDescription = tokenDescription;
      this.SetVisualStudioThemeStyles();
    }

    public override string TokenValue
    {
      get => TokenValueInputBox.Text;
      set => TokenValueInputBox.Text = value;
    }

    public override Label DisplayNameLabel => TokenDisplayNameLabel;
    public override Control InputControl => TokenValueInputBox;
    public override TextBlock HelpTextBlock => TokenHelpTextBlock;
  }
}