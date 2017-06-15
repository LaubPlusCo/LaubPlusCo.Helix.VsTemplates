using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  /// Interaction logic for TokenTextInput.xaml
  /// </summary>
  public partial class TokenTextInput : TokenInputControl
  {
    public TokenTextInput(TokenDescription tokenDescription)
    {
      InitializeComponent();
      TokenValueInputBox.TextChanged += InputChangedEventHandler;
      TokenDescription = tokenDescription;
    }

    public override string TokenValue
    {
      get => TokenValueInputBox.Text;
      set => TokenValueInputBox.Text = value;
    }

    public override Label DisplayNameLabel => TokenDisplayNameLabel;
    public override Control InputControl => TokenValueInputBox;
  }
}
