using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;
using Microsoft.VisualStudio.PlatformUI;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  /// <summary>
  ///   Interaction logic for ManifestDialog.xaml
  /// </summary>
  public partial class ManifestDialog : DialogWindow
  {
    private HelixTemplateManifest[] _manifests;

    private IDictionary<string, string> _replacementTokens;

    private HelixTemplateManifest _selectedManifest;

    private bool? _isSolutionCreation;

    public ManifestDialog(string helpTopic) : base(helpTopic)
    {
      InitializeComponent();
      DataContext = this;
    }

    public ManifestDialog()
    {
      InitializeComponent();
      DataContext = this;
    }

    public IHelixProjectTemplate HelixProjectTemplate { get; protected set; }
    public ObservableCollection<ComboBoxItem> AvailableManifestsCollection { get; set; }
    public ComboBoxItem SelectedManifestComboItem { get; set; }
    public string SolutionRoot { get; protected set; }

    public IEnumerable<TokenDescription> TokenDescriptions { get; set; }

    public void Initialize(string rootDirectory, string solutionRoot, IDictionary<string, string> replacementTokens, bool? isSolutionCreation)
    {
      _replacementTokens = replacementTokens;
      _isSolutionCreation = isSolutionCreation;
      SolutionRoot = solutionRoot;
      var readAllManifestService = new ReadAllManifestFilesService(rootDirectory, _replacementTokens);
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
      AvailableManifests.SelectedIndex = 0;
    }

    protected void SetAvailableManifestsCollection(IEnumerable<HelixTemplateManifest> helixTemplateManifests)
    {
      AvailableManifestsCollection = new ObservableCollection<ComboBoxItem>();
      foreach (var helixTemplateManifest in helixTemplateManifests)
        AvailableManifestsCollection.Add(new ComboBoxItem
        {
          Content = helixTemplateManifest.Name + " " + helixTemplateManifest.Version
        });
      AvailableManifests.SelectionChanged += SelectionChanged;
    }

    protected void SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
      SelectedManifestComboItem = AvailableManifestsCollection[AvailableManifests.SelectedIndex];
      _selectedManifest = _manifests[AvailableManifests.SelectedIndex];
      SetSelectedManifest();
    }

    protected void SetSelectedManifest()
    {
      InputControlsPanel.Children.Clear();

      var tokenInputs = _selectedManifest.Tokens.Select(t => new TokenInput() { TokenDescription = t }).ToArray();
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

    protected void SettingsButton_Clicked(object sender, RoutedEventArgs e)
    {
      var settingsDialog = new SettingsDialog();
      var settingsUpdated = settingsDialog.ShowDialog();
      if (!settingsUpdated.HasValue || !settingsUpdated.Value)
        return;
      var rootDirectory = settingsDialog.RootDirectory;
      Initialize(rootDirectory, SolutionRoot, _replacementTokens, _isSolutionCreation);
      _selectedManifest = _manifests[AvailableManifests.SelectedIndex];
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
        _replacementTokens.Add(tokenInput.TokenKey, tokenInput.TokenValue);
      HelixProjectTemplate = _selectedManifest.TemplateEngine.Run(_selectedManifest, SolutionRoot, _replacementTokens);
      if (HelixProjectTemplate == null)
        DialogResult = false;
      DialogResult = true;
      Close();
    }


    private IEnumerable<TokenInput> GetTokenInputs()
    {
      foreach (var inputChild in InputControlsPanel.Children)
      {
        var tokenInput = (TokenInput)inputChild;
        if (tokenInput == null) continue;
        yield return tokenInput;
      }
    }

    private void HelixLogo_Clicked(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://helix.sitecore.net"));
      e.Handled = true;
    }

    private void LaubLogo_Clicked(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://laubplusco.net"));
      e.Handled = true;
    }

    private void GithubLogo_Clicked(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://github.com/LaubPlusCo/Helix.VsTemplates"));
      e.Handled = true;
    }
  }
}