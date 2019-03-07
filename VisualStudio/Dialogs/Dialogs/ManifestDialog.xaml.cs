using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;
using LaubPlusCo.Foundation.HelixTemplating.Tokens;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.PlatformUI;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for ManifestDialog.xaml
  /// </summary>
  public partial class ManifestDialog : DialogWindow
  {
    private HelixTemplateManifest[] _manifests;

    private HelixTemplateManifest _selectedManifest;

    private IDictionary<string, string> _initialTokens;

    private ModuleTemplateFolderService _moduleTemplateFolderService;

    private bool? _isSolutionCreation;

    public ManifestDialog(string helpTopic, ModuleTemplateFolderService moduleTemplateFolderService) : base(helpTopic)
    {
      InitializeComponent();
      DataContext = this;
    }

    public ManifestDialog(ModuleTemplateFolderService moduleTemplateFolderService)
    {
      InitializeComponent();
      DataContext = this;
    }

    public IHelixProjectTemplate HelixProjectTemplate { get; protected set; }
    public ObservableCollection<ComboBoxItem> AvailableManifestsCollection { get; set; }
    public ComboBoxItem SelectedManifestComboItem { get; set; }
    public string SolutionRoot { get; protected set; }

    public IEnumerable<TokenDescription> TokenDescriptions { get; set; }

    public void Initialize(string rootDirectory, string solutionRoot, IDictionary<string, string> initialTokens, bool? isSolutionCreation)
    {
      _initialTokens = initialTokens.ToDictionary(t => t.Key, t => t.Value);
      _isSolutionCreation = isSolutionCreation;
      SolutionRoot = solutionRoot;

      if (isSolutionCreation.HasValue && !isSolutionCreation.Value)
      {
        _moduleTemplateFolderService = new ModuleTemplateFolderService(solutionRoot);
        var relativeModuleTemplateFolder = _moduleTemplateFolderService.Locate();
        if (!string.IsNullOrWhiteSpace(relativeModuleTemplateFolder)) ;
        rootDirectory = relativeModuleTemplateFolder;
      }

      var readAllManifestService = new ReadAllManifestFilesService(rootDirectory, initialTokens);
      _manifests = readAllManifestService.Read();

      if (isSolutionCreation.HasValue)
      {
        _manifests = _manifests.Where(m => isSolutionCreation.Value ? m.TemplateType == TemplateType.Solution : m.TemplateType != TemplateType.Solution).ToArray();
      }

      if (!_manifests.Any())
      {
        MessageBox.Show("No valid templates found in root directory", "Information", MessageBoxButton.OK);
        return;
      }
      SetAvailableManifestsCollection(_manifests);
      AvailableManifestsComboBox.Items.Refresh();
      AvailableManifestsComboBox.SelectedIndex = 0;
    }

    protected void SetAvailableManifestsCollection(IEnumerable<HelixTemplateManifest> helixTemplateManifests)
    {
      AvailableManifestsCollection = new ObservableCollection<ComboBoxItem>();
      foreach (var helixTemplateManifest in helixTemplateManifests)
        AvailableManifestsCollection.Add(new ComboBoxItem
        {
          Content = helixTemplateManifest.Name + " " + helixTemplateManifest.Version
        });
      AvailableManifestsComboBox.SelectionChanged += SelectionChanged;
    }

    protected void SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
      SelectedManifestComboItem = AvailableManifestsCollection[AvailableManifestsComboBox.SelectedIndex];
      _selectedManifest = _manifests[AvailableManifestsComboBox.SelectedIndex];
      SetSelectedManifest();
    }

    protected void SetSelectedManifest()
    {
      InputControlsPanel.Children.Clear();

      var tokenInputs = _selectedManifest.Tokens.Select(GetTokenInputControl).ToArray();
      foreach (var dependentSuggestion in tokenInputs.Where(t => t.Suggestor != null && t.Suggestor.DependentOnKeys.Any()))
      {
        var inputs = tokenInputs.Where(input => dependentSuggestion.Suggestor.DependentOnKeys.Contains(input.TokenKey));
        dependentSuggestion.DependendTokenInputs = inputs.ToArray();
      }

      foreach (var tokenInput in tokenInputs)
      {
        tokenInput.Initialize();
        InputControlsPanel.Children.Add(tokenInput);
      }

      TemplateDescription.Text = _selectedManifest.Description;
      TemplateAuthor.Text = $"author: {_selectedManifest.Author}";
    }

    private TokenInputControl GetTokenInputControl(TokenDescription tokenDescription)
    {
      switch (tokenDescription.InputType)
      {
        case TokenInputForm.Text:
          return new TokenTextInput(tokenDescription);
        case TokenInputForm.Folder:
          return new TokenFolderInput(tokenDescription);
        case TokenInputForm.Selection:
          return new TokenSelectionInput(tokenDescription);
        default:
          return new TokenTextInput(tokenDescription);
      }
    }

    protected void SettingsButton_Clicked(object sender, RoutedEventArgs e)
    {
      var settingsDialog = new SettingsDialog();
      var settingsUpdated = settingsDialog.ShowDialog();
      if (!settingsUpdated.HasValue || !settingsUpdated.Value)
        return;

      var rootDirectory = settingsDialog.RootDirectory;
      Initialize(rootDirectory, SolutionRoot, _initialTokens, _isSolutionCreation);
      _selectedManifest = _manifests[AvailableManifestsComboBox.SelectedIndex];
      SetSelectedManifest();
    }

    private void SelectButton_OnClick(object sender, RoutedEventArgs e)
    {
      var tokenInputs = GetTokenInputs().ToArray();
      var validationResults = tokenInputs.Select(ti => ti.Validate()).ToList();

      if (validationResults.Any(vr => !vr.IsValid))
      {
        var message = $"{string.Join("\n\n", validationResults.Where(vr => !vr.IsValid).Select(vr => $"> {vr.Message}"))}";
        MessageBox.Show(message, "Token validation errors", MessageBoxButton.OK);
        return;
      }

      foreach (var tokenInput in tokenInputs)
      {
        if (_selectedManifest.ReplacementTokens.ContainsKey(tokenInput.TokenKey))
        { 
          _selectedManifest.ReplacementTokens[tokenInput.TokenKey] = tokenInput.TokenValue;
          continue;
        }
        _selectedManifest.ReplacementTokens.Add(tokenInput.TokenKey, tokenInput.TokenValue);
      }

      HelixProjectTemplate = _selectedManifest.TemplateEngine.Run(_selectedManifest, SolutionRoot);
      if (HelixProjectTemplate == null)
        DialogResult = false;
      DialogResult = true;
      Close();
    }


    private IEnumerable<TokenInputControl> GetTokenInputs()
    {
      foreach (var inputChild in InputControlsPanel.Children)
      {
        var tokenInput = (TokenInputControl)inputChild;
        if (tokenInput == null) continue;
        yield return tokenInput;
      }
    }

    private void HelixLogo_Clicked(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("http://helix.sitecore.net"));
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