using System.IO;
using System.Windows;
using System.Windows.Forms;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using Control = System.Windows.Controls.Control;
using Label = System.Windows.Controls.Label;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for TokenFolderInput.xaml
  /// </summary>
  public partial class TokenFolderInput : TokenInputControl
  {
    public TokenFolderInput(TokenDescription tokenDescription)
    {
      InitializeComponent();
      TokenValueAsPathInputBox.TextChanged += InputChangedEventHandler;
      TokenDescription = tokenDescription;
    }

    public override string TokenValue
    {
      get => TokenValueAsPathInputBox.Text;
      set => TokenValueAsPathInputBox.Text = value;
    }

    public override Label DisplayNameLabel => TokenDisplayNameLabel;
    public override Control InputControl => TokenValueAsPathInputBox;

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