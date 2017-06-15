using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;
using LaubPlusCo.Foundation.HelixTemplating.Tokens;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class ParseManifestService
  {
    protected ReplaceTokensService ReplaceTokensService;
    public ParseManifestService(string manifestFilePath)
    {
      ManifestFilePath = manifestFilePath;
      if (!File.Exists(ManifestFilePath))
        throw new ManifestParseException($"Could not find Manifest file {ManifestFilePath}");
    }

    protected virtual XPathNavigator RootNavigator { get; set; }
    protected virtual HelixTemplateManifest Manifest { get; set; }
    protected string ManifestFilePath { get; set; }
    protected virtual ManifestTypeInstantiator ManifestTypeInstantiator { get; set; }

    public virtual HelixTemplateManifest Parse(IDictionary<string, string> replacementTokens)
    {
      Manifest = new HelixTemplateManifest(ManifestFilePath) {ReplacementTokens = replacementTokens};
      ManifestTypeInstantiator = new ManifestTypeInstantiator();
      return Parse(File.ReadAllText(ManifestFilePath));
    }

    protected HelixTemplateManifest Parse(string fileContent)
    {
      var xmlDocumentdoc = new XPathDocument(XmlReader.Create(new StringReader(fileContent),
        new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }));
      RootNavigator = xmlDocumentdoc.CreateNavigator();
      ParseManifestInformation();
      ParseTemplateEngine();
      ParseReplacementTokens();
      ParseProjectsToAttach();
      ParseSkipAttach();
      ParseVirtualSolutionFolders();
      ParseIgnoreFiles();
      ParseType();
      return Manifest;
    }

    private void ParseSkipAttach()
    {
      var skipAttachPaths = GetNodeByXPath("/templateManifest/skipAttach");
      if (skipAttachPaths == null) return;
      Manifest.SkipAttachPaths = GetPaths(skipAttachPaths).Concat(GetPaths(skipAttachPaths, "folder")).ToList(); ;
    }

    private void ParseType()
    {
      var templateManifestNode = GetNodeByXPath("/templateManifest");
      if (!templateManifestNode.HasAttributes)
      { 
        Manifest.TemplateType = TemplateType.Module;
        return;
      }
      var typeOfTemplate = templateManifestNode.GetAttribute("typeOfTemplate", "");
      if (string.IsNullOrEmpty(typeOfTemplate))
      {
        Manifest.TemplateType = TemplateType.Module;
        return;
      }
      Manifest.TemplateType = (TemplateType) Enum.Parse(typeof(TemplateType), typeOfTemplate);
    }

    private void ParseIgnoreFiles()
    {
      var ignoreFilesNavigator = GetNodeByXPath("/templateManifest/ignoreFiles");
      if (ignoreFilesNavigator == null) return;
      Manifest.IgnoreFiles = GetPaths(ignoreFilesNavigator);
    }

    private void ParseVirtualSolutionFolders()
    {
      var virtualSolutionFoldersNavigator = GetNodeByXPath("/templateManifest/virtualSolutionFolders");
      if (virtualSolutionFoldersNavigator == null) return;
      Manifest.VirtualSolutionFolders = GetVirtualSolutionFolders(virtualSolutionFoldersNavigator);

    }

    private IList<VirtualSolutionFolder> GetVirtualSolutionFolders(XPathNavigator virtualSolutionFoldersNavigator)
    {
      var virtualSolutionFolders = new List<VirtualSolutionFolder>();
      foreach (XPathNavigator tokenNavigator in virtualSolutionFoldersNavigator.SelectChildren("virtualSolutionFolder", ""))
      {
        virtualSolutionFolders.Add(new VirtualSolutionFolder
        {
          Name = tokenNavigator.GetAttribute("name", ""),
          Files = GetPaths(tokenNavigator),
          SubFolders = GetVirtualSolutionFolders(tokenNavigator)
        });
      }
      return virtualSolutionFolders;
    }

    private IList<string> GetPaths(XPathNavigator tokenNavigator, string attributeName = "file")
    {
      var paths = new List<string>();
      foreach (XPathNavigator fileNavigator in tokenNavigator.SelectChildren(attributeName, ""))
      {
        var filePath = fileNavigator.GetAttribute("path", "");
        if (string.IsNullOrEmpty(filePath)) throw new ManifestParseException("Missing path attribute on element.");
        paths.Add(GetFullPath(filePath));
      }
      return paths;
    }

    private void ParseProjectsToAttach()
    {
      var projectFileNavigator = GetRequiredNodeByXPath("/templateManifest/projectsToAttach");
      foreach (XPathNavigator tokenNavigator in projectFileNavigator.SelectChildren("projectFile", ""))
      {
        var projectFilePath = tokenNavigator.GetAttribute("path", "");
        if (string.IsNullOrEmpty(projectFilePath)) throw new ManifestParseException("Missing path attribute on project file to attach");
        Manifest.ProjectsToAttach.Add(GetFullPath(projectFilePath));
      }
    }

    private void ParseReplacementTokens()
    {
      var replacemenTokenNavigator = GetRequiredNodeByXPath("/templateManifest/replacementTokens");
      foreach (XPathNavigator tokenNavigator in replacemenTokenNavigator.SelectChildren("token", ""))
      {
        var selectionOptions = GetTokenSelectOptions(tokenNavigator);
        var tokenInputType = selectionOptions.Any() ? TokenInputForm.Selection 
          :  Enum.TryParse(tokenNavigator.GetAttribute("input", ""), out TokenInputForm inputFormat) ? inputFormat : TokenInputForm.Text;

        var tokenDescription = new TokenDescription
        {
          DisplayName = tokenNavigator.GetAttribute("displayName", ""),
          Key = tokenNavigator.GetAttribute("key", ""),
          Default = tokenNavigator.GetAttribute("default", ""),
          InputType = tokenInputType,
          SelectionOptions = selectionOptions,
          IsRequired = GetBooleanAttribute("required", tokenNavigator, true)
        };

        var validatorType = tokenNavigator.GetAttribute("validationType", "");
        if (!string.IsNullOrEmpty(validatorType))
          tokenDescription.Validator = ManifestTypeInstantiator.CreateInstance<IValidateToken>(validatorType);
        var suggestorType = tokenNavigator.GetAttribute("suggestType", "");
        if (!string.IsNullOrEmpty(suggestorType))
          tokenDescription.Suggestor = ManifestTypeInstantiator.CreateInstance<ISuggestToken>(suggestorType);
        Manifest.Tokens.Add(tokenDescription);
      }
      ExpandDefaultValues(0);
    }

    private void ExpandDefaultValues(int noOfRuns)
    {
      foreach (var tokenDescription in Manifest.Tokens.Where(t => !string.IsNullOrEmpty(t.Default) && !t.Default.Contains("$")).ToArray())
      {
        if (Manifest.ReplacementTokens.ContainsKey(tokenDescription.Key)) continue;
        Manifest.ReplacementTokens.Add(tokenDescription.Key, tokenDescription.Default);
      }
      ReplaceTokensService = new ReplaceTokensService(Manifest.ReplacementTokens);
      foreach (var tokenDescription in Manifest.Tokens.Where(t => !string.IsNullOrEmpty(t.Default) && t.Default.Contains("$")))
      {
        tokenDescription.Default = ReplaceTokensService.Replace(tokenDescription.Default);
      }
      if (!Manifest.Tokens.Any(t => t.Default.Contains("$")) || noOfRuns >= 4) return;
      noOfRuns++;
      ExpandDefaultValues(noOfRuns);
    }

    private KeyValuePair<string, string>[] GetTokenSelectOptions(XPathNavigator tokenNavigator)
    {
      var tokenSelectOptions = new Dictionary<string,string>();
      foreach (XPathNavigator optionNavigator in tokenNavigator.SelectChildren("option", ""))
      {
        var key = optionNavigator.GetAttribute("key", "");
        var name = optionNavigator.GetAttribute("name", "");
        if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(name)) throw new ManifestParseException("Token select option miss both name and key");
        key = string.IsNullOrWhiteSpace(key) ? name : key;
        if (tokenSelectOptions.ContainsKey(key)) throw new ManifestParseException($"Token select options has duplicate key {key}");
        name = string.IsNullOrWhiteSpace(name) ? key : name;
        tokenSelectOptions.Add(key, name);
      }
      return tokenSelectOptions.ToArray();
    }

    private bool GetBooleanAttribute(string attr, XPathNavigator navigator, bool defaultValue)
    {
      var value = navigator.GetAttribute(attr, "");
      if (string.IsNullOrEmpty(value)) return defaultValue;
      bool.TryParse(value, out defaultValue);
      return defaultValue;
    }

    private void ParseTemplateEngine()
    {
      var type = GetRequiredValue("/templateManifest/templateEngine/@type");
      Manifest.TemplateEngine = ManifestTypeInstantiator.CreateInstance<IHelixTemplateEngine>(type);
    }

    private void ParseManifestInformation()
    {
      Manifest.Name = GetRequiredValue("/templateManifest/name");
      Manifest.Description = GetRequiredValue("/templateManifest/description");
      Manifest.Version = GetRequiredValue("/templateManifest/version");
      Manifest.Author = GetRequiredValue("/templateManifest/author");
      Manifest.SourceFolder = GetFullPath(GetRequiredValue("/templateManifest/sourceFolder"));
      Manifest.SaveOnCreate = bool.TryParse(GetRequiredValue("/templateManifest/saveOnCreate"), out bool saveOnCreate) && saveOnCreate;
    }

    private string GetFullPath(string path)
    {
      path = path.Replace("/", @"\");
      var fullPath = path.StartsWith(Manifest.ManifestRootPath) ? path : CombinePaths(Manifest.ManifestRootPath, path);
      if (!File.Exists(fullPath) && !Directory.Exists(fullPath)) throw new ManifestParseException($"Could not find file or directory on {path} - expected full path {fullPath}");
      return fullPath;
    }

    private string CombinePaths(string path1, string path2)
    {
      var pathParts = path2.Split('\\').Where(s => !string.IsNullOrEmpty(s)).ToList();
      if (path1[path1.Length - 1] == '\\')
        path1 = path1.Remove(path1.Length - 1);
      pathParts.Insert(0, path1);
      return string.Join(@"\", pathParts);
    }

    private string GetRequiredValue(string xPath)
    {
      return GetRequiredNodeByXPath(xPath).Value;
    }

    private XPathNavigator GetNodeByXPath(string xPath)
    {
      return RootNavigator.SelectSingleNode(xPath);
    }

    private XPathNavigator GetRequiredNodeByXPath(string xPath)
    {
      var xPathNavigator = GetNodeByXPath(xPath);
      if (xPathNavigator != null) return xPathNavigator;
      throw new ManifestParseException(xPath);
    }
  }
}