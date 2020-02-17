using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;
using LaubPlusCo.Foundation.HelixTemplating.Tokens;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.Shell;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for ManifestDialog.xaml
  /// </summary>
  public partial class ManifestDialog
  {
    private readonly TextTraceListener _traceListener;
    private IDictionary<string, string> _initialTokens;
    private bool _isSolutionCreation;
    private HelixTemplateManifest[] _manifests;
    private HelixTemplateManifest _selectedManifest;
    private string _solutionRoot;

    private SolutionScopeSettings _solutionScopeSettings;

    public TraceWindow TraceWindow;

    public ManifestDialog()
    {
      _traceListener = new TextTraceListener {TraceOutputOptions = TraceOptions.DateTime};
      InitializeComponent();
      DataContext = this;
      this.SetVisualStudioThemeStyles();
      TokenSectionTabs.Background = Background;
      TokenSectionTabs.BorderThickness = new Thickness(1, 0, 0, 0);
      TokenSectionTabs.BorderBrush = (Brush) FindResource(VsBrushes.PanelHyperlinkKey);
      TokenSectionTabs.Margin = new Thickness(0, 5, 15, 5);
      TokenSectionTabs.Style = Style;
    }

    public IHelixProjectTemplate HelixProjectTemplate { get; protected set; }
    public ObservableCollection<ComboBoxItem> AvailableManifestsCollection { get; set; }

    public void Initialize(string globalTemplatesFolder, string solutionRoot, IDictionary<string, string> initialTokens,
      bool isSolutionCreation)
    {
      _initialTokens = initialTokens.ToDictionary(t => t.Key, t => t.Value);
      _isSolutionCreation = isSolutionCreation;
      _solutionRoot = solutionRoot;

      if (!_isSolutionCreation)
        _solutionScopeSettings = new SolutionScopeSettings(solutionRoot);

      var typeText = isSolutionCreation ? "solution" : "module";
      HeadlineText.Text = $"Create new {typeText}";

      var templatesFolder = _isSolutionCreation
        ? globalTemplatesFolder
        : FindModuleTemplatesRootDirectory(solutionRoot, globalTemplatesFolder);

      _manifests = new ReadAllManifestFilesService(templatesFolder, initialTokens).Read();
      _manifests = _manifests.Where(m =>
          (m.TemplateType == TemplateType.Solution) & isSolutionCreation ||
          (m.TemplateType != TemplateType.Solution) & !isSolutionCreation)
        .ToArray();

      if (!_manifests.Any())
      {
        MessageBox.Show("No valid templates found in root directory\nSee Trace window for details.", "Information",
          MessageBoxButton.OK);
        return;
      }

      SetAvailableManifestsCollection(_manifests);
      AvailableManifestsComboBox.SelectedIndex = 0;
    }

    private string FindModuleTemplatesRootDirectory(string solutionRoot, string globalTemplatesFolder)
    {
      var locateTemplateFolderService = new TemplateFolderService(solutionRoot);

      if (locateTemplateFolderService.TryGetAbsolutePath(_solutionScopeSettings.RelativeTemplatesFolder,
        out var templateFolderfullpath))
        return templateFolderfullpath;

      var moduleTemplateFolder = locateTemplateFolderService.Locate();
      if (!string.IsNullOrWhiteSpace(moduleTemplateFolder))
      {
        _solutionScopeSettings.RelativeTemplatesFolder = moduleTemplateFolder;
        _solutionScopeSettings.SaveSettings();
        return moduleTemplateFolder;
      }

      if (_solutionScopeSettings.SkipCreateFolderDialog)
        return globalTemplatesFolder;

      var createFolderResult = MessageBox.Show(
        "The current solution does not have a local module templates folder with valid templates.\n\nDo you want to create one and unzip the example templates?",
        "Create solution scope Helix modules template folder", MessageBoxButton.YesNo);

      if (createFolderResult != MessageBoxResult.Yes)
      {
        var skipDialogResult = MessageBox.Show(
          "It is recommended to keep module templates under source control together with the solution.\n\nYou can manually create a folder in the solution root directory and copy in your Helix module templates.\n\nDo you want to skip this dialog in the future for the current solution?",
          "Create a solution scope Helix modules template folder", MessageBoxButton.YesNo);
        _solutionScopeSettings.SkipCreateFolderDialog = skipDialogResult == MessageBoxResult.Yes;
        _solutionScopeSettings.RelativeTemplatesFolder = string.Empty;
        _solutionScopeSettings.SaveSettings();
        return globalTemplatesFolder;
      }

      moduleTemplateFolder = TemplateInstallService.CreateTemplateFolder(solutionRoot);
      foreach (var manifest in _manifests.Where(m => m.TemplateType != TemplateType.Solution))
        TemplateInstallService.CopyDirectory(new DirectoryInfo(manifest.ManifestRootPath), moduleTemplateFolder);
      _solutionScopeSettings.RelativeTemplatesFolder = moduleTemplateFolder;
      return moduleTemplateFolder;
    }

    protected void SetAvailableManifestsCollection(IEnumerable<HelixTemplateManifest> helixTemplateManifests)
    {
      AvailableManifestsCollection = new ObservableCollection<ComboBoxItem>();

      foreach (var helixTemplateManifest in helixTemplateManifests)
        AvailableManifestsCollection.Add(new ComboBoxItem
        {
          Content = helixTemplateManifest.Name
        });
      AvailableManifestsComboBox.ItemsSource = AvailableManifestsCollection;
    }

    protected void SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
      if (AvailableManifestsComboBox.SelectedIndex < 0 ||
          AvailableManifestsComboBox.SelectedIndex > AvailableManifestsCollection.Count)
        return;
      _selectedManifest = _manifests[AvailableManifestsComboBox.SelectedIndex];
      SetSelectedManifest();
    }

    protected void AddTokenInputControls(Panel panel, IList<ITokenDescription> tokenDescriptions)
    {
      panel.Children.Clear();
      var tokenInputs = tokenDescriptions.Select(GetTokenInputControl).ToArray();
      foreach (var dependentSuggestion in tokenInputs.Where(t =>
        t.Suggestor != null && t.Suggestor.DependentOnKeys.Any()))
      {
        var inputs = tokenInputs.Where(input => dependentSuggestion.Suggestor.DependentOnKeys.Contains(input.TokenKey));
        dependentSuggestion.DependendTokenInputs = inputs.ToArray();
      }

      foreach (var tokenInput in tokenInputs)
      {
        tokenInput.Initialize();
        panel.Children.Add(tokenInput);
      }
    }

    protected void SetSelectedManifest()
    {
      TokenSectionTabs.Items.Clear();
      AddTokenSectionTabItem(_selectedManifest.TemplateType.ToString(), _selectedManifest.Tokens);
      if (_selectedManifest.TokenSections != null && _selectedManifest.TokenSections.Any())
        foreach (var tokenSection in _selectedManifest.TokenSections)
          AddTokenSectionTabItem(tokenSection.DisplayName, tokenSection.Tokens);
      if (AppScopeSettings.Current.ShowVsTokensTab)
        AddVsTokensTab();

      TokenSectionTabs.SelectedIndex = 0;

      TemplateDescription.Text =
        string.Join("\n", _selectedManifest.Description.Split('\n').Select(s => s.TrimStart()));
      var templateMetaText = $"template v{_selectedManifest.Version}, author: {_selectedManifest.Author}";
      WriteTraceService.WriteToTrace($"Manifest \"{_selectedManifest.Name}\" selected");

      if (_selectedManifest.Link != null)
      {
        var templateLink = new Hyperlink
        {
          NavigateUri = _selectedManifest.Link.LinkUri
        };
        templateLink.RequestNavigate += (sender, e) =>
        {
          Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
          e.Handled = true;
        };
        templateLink.Inlines.Add(string.IsNullOrEmpty(_selectedManifest.Link.LinkText)
          ? "Link.."
          : _selectedManifest.Link.LinkText);
        TemplateAuthor.Inlines.Clear();
        TemplateAuthor.Inlines.Add(templateLink);
        TemplateAuthor.Inlines.Add($"\t{templateMetaText}");
        return;
      }

      TemplateAuthor.Text = templateMetaText;
    }

    private void AddVsTokensTab()
    {
      var tabItem = new TokenSectionTabItem("VS Tokens");
      TokenSectionTabs.Items.Add(tabItem);
      foreach (var token in _initialTokens)
        tabItem.InnerPanel.Children.Add(new TextBlock
        {
          Text = $"{token.Key}:   {token.Value}",
          Foreground = (Brush) FindResource(VsBrushes.CaptionTextKey)
        });
    }

    private void AddTokenSectionTabItem(string displayName, IList<ITokenDescription> tokenDescriptions)
    {
      var tabItem = new TokenSectionTabItem(displayName);
      TokenSectionTabs.Items.Add(tabItem);
      AddTokenInputControls(tabItem.InnerPanel, tokenDescriptions);
    }

    private TokenInputControl GetTokenInputControl(ITokenDescription tokenDescription)
    {
      switch (tokenDescription.InputType)
      {
        case TokenInputForm.Text:
          return new TokenTextInput(tokenDescription);
        case TokenInputForm.Folder:
          return new TokenFolderInput(tokenDescription);
        case TokenInputForm.Selection:
          return new TokenSelectionInput(tokenDescription);
        case TokenInputForm.Checkbox:
          return new TokenCheckboxInput(tokenDescription);
        default:
          return new TokenTextInput(tokenDescription);
      }
    }

    protected void SettingsButton_Clicked(object sender, RoutedEventArgs e)
    {
      var settingsDialog = new SettingsDialog(_solutionRoot);
      var settingsUpdated = settingsDialog.ShowDialog();
      if (!settingsUpdated.HasValue || !settingsUpdated.Value)
        return;

      Initialize(settingsDialog.GlobalTemplateFolder, _solutionRoot, _initialTokens, _isSolutionCreation);
    }

    protected void OpenTrace_Clicked(object sender, RoutedEventArgs e)
    {
      if (TraceWindow == null)
        TraceWindow = new TraceWindow(_traceListener);
      if (!TraceWindow.IsVisible)
        TraceWindow.Show();
    }

    private void CreateButton_OnClick(object sender, RoutedEventArgs e)
    {
      var tokenInputs = GetTokenInputs().ToArray();
      var validationResults = tokenInputs.Select(ti => ti.Validate()).ToList();

      if (validationResults.Any(vr => !vr.IsValid))
      {
        var message =
          $"{string.Join("\n\n", validationResults.Where(vr => !vr.IsValid).Select(vr => $"> {vr.Message}"))}";
        WriteTraceService.WriteToTrace(message);
        MessageBox.Show(message, "Token validation errors", MessageBoxButton.OK);
        return;
      }

      WriteTraceService.WriteToTrace("\nReading token inputs\n-----------");
      foreach (var tokenInput in tokenInputs)
      {
        WriteTraceService.WriteToTrace($"Input - {tokenInput.TokenKey}: {tokenInput.TokenValue}");
        if (_selectedManifest.ReplacementTokens.ContainsKey(tokenInput.TokenKey))
        {
          _selectedManifest.ReplacementTokens[tokenInput.TokenKey] = tokenInput.TokenValue;
          continue;
        }

        _selectedManifest.ReplacementTokens.Add(tokenInput.TokenKey, tokenInput.TokenValue);
      }

      WriteTraceService.WriteToTrace("All template replacement tokens:\n-----------", "\nInfo",
        _selectedManifest.ReplacementTokens.Select(t => $"{t.Key}: {t.Value}").ToArray());

      WriteTraceService.WriteToTrace("Running template engine:\n-----------", "\nInfo");
      HelixProjectTemplate = _selectedManifest.TemplateEngine.Run(_selectedManifest, _solutionRoot);

      if (HelixProjectTemplate == null)
      {
        DialogResult = false;
        WriteTraceService.WriteToTrace("Project creation failed. See output in above trace", "\nError");
        Close();
      }

      DialogResult = true;
      WriteTraceService.WriteToTrace(_isSolutionCreation ? "Solution created.." : "Module created..");
      Close();
    }

    protected override void OnClosed(EventArgs e)
    {
      if (TraceWindow != null && TraceWindow.IsVisible)
      {
        WriteTraceService.WriteToTrace("Please close this window..", "\nDone");
        TraceWindow.Focus();
      }

      if (_traceListener != null && !_traceListener.IsDisposed)
        _traceListener.Dispose();
      base.OnClosed(e);
    }

    private IEnumerable<TokenInputControl> GetTokenInputs()
    {
      foreach (var tabItem in TokenSectionTabs.Items)
      {
        if (!(tabItem is TokenSectionTabItem tokenSectionTabItem)) continue;
        foreach (var inputChild in tokenSectionTabItem.InnerPanel.Children)
        {
          var tokenInput = (TokenInputControl) inputChild;
          if (tokenInput == null) continue;
          yield return tokenInput;
        }
      }
    }

    private void HelixLogo_Clicked(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://helix.sitecore.net"));
      e.Handled = true;
    }

    private void LaubLogo_Clicked(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://laubplusco.net/?origin=vstemplates"));
      e.Handled = true;
    }

    private void GithubLogo_Clicked(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://github.com/LaubPlusCo/Helix.VsTemplates"));
      e.Handled = true;
    }
  }
}