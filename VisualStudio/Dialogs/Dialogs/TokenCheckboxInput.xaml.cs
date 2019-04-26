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
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  /// Interaction logic for TokenCheckboxInput.xaml
  /// </summary>
  public partial class TokenCheckboxInput : TokenInputControl
  {
    public TokenCheckboxInput(TokenDescription tokenDescription)
    {
      InitializeComponent();
      TokenDescription = tokenDescription;
      this.SetVisualStudioThemeStyles();
    }

    public override string TokenValue
    {
      get => TokenInputCheckbox.IsChecked.HasValue && TokenInputCheckbox.IsChecked.Value ? bool.TrueString : bool.FalseString;
      set => TokenInputCheckbox.IsChecked = bool.TryParse(value, out bool val) && val;
    }

    public override Label DisplayNameLabel => TokenDisplayNameLabel;
    public override Control InputControl => TokenInputCheckbox;
    public override TextBlock HelpTextBlock => TokenHelpTextBlock;
  }
}
