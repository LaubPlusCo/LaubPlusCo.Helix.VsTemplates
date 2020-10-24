using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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
      if (File.Exists(ManifestFilePath)) return;
      Trace.WriteLine($"Could not find Manifest file {ManifestFilePath}");
      throw new ManifestParseException($"Could not find Manifest file {ManifestFilePath}");
    }

    protected IEvaluateCondition DefaultConditionEvaluator = new BooleanConditionEvaluator();

    protected virtual XPathNavigator RootNavigator { get; set; }

    protected virtual HelixTemplateManifest Manifest { get; set; }

    protected string ManifestFilePath { get; set; }

    protected virtual ManifestTypeInstantiator ManifestTypeInstantiator { get; set; }

    public virtual HelixTemplateManifest Parse(IDictionary<string, string> replacementTokens)
    {
      try
      {
        Manifest = new HelixTemplateManifest(ManifestFilePath) { ReplacementTokens = replacementTokens };
        ManifestTypeInstantiator = new ManifestTypeInstantiator();
        return Parse(File.ReadAllText(ManifestFilePath));
      }
      catch (Exception exception)
      {
        Trace.WriteLine($"Exception occurred while parsing manifest: {exception.Message}\n\n{exception.StackTrace}");
        return null;
      }
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
      ParseIgnorePaths();
      ParseType();
      return Manifest;
    }

    protected virtual void ParseSkipAttach()
    {
      var skipAttachPaths = GetNodeByXPath("/templateManifest/skipAttach");
      if (skipAttachPaths == null) return;
      Manifest.SkipAttachPaths = GetPathValues(skipAttachPaths)
        .Concat(GetPathValues(skipAttachPaths, "folder"))
        .Concat(GetPathValues(skipAttachPaths, "path")).ToList(); ;
    }

    protected virtual void ParseType()
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
      Manifest.TemplateType = (TemplateType)Enum.Parse(typeof(TemplateType), typeOfTemplate);
    }

    protected virtual void ParseIgnorePaths()
    {
      var ignoreNavigator = GetIgnoredPathsNavigator();
      if (ignoreNavigator == null) return;
      Manifest.IgnorePaths = GetPathValues(ignoreNavigator)
        .Concat(GetPathValues(ignoreNavigator, "folder"))
        .Concat(GetPathValues(ignoreNavigator, "path")).ToList();
    }

    protected XPathNavigator GetIgnoredPathsNavigator()
    {
      var ignoreNavigator = GetNodeByXPath("/templateManifest/ignoreFiles");
      if (ignoreNavigator != null) return ignoreNavigator;
      ignoreNavigator = GetNodeByXPath("/templateManifest/ignorePaths");
      return ignoreNavigator;
    }

    protected virtual void ParseVirtualSolutionFolders()
    {
      var virtualSolutionFoldersNavigator = GetNodeByXPath("/templateManifest/virtualSolutionFolders");
      if (virtualSolutionFoldersNavigator == null) return;
      Manifest.VirtualSolutionFolders = GetVirtualSolutionFolders(virtualSolutionFoldersNavigator);

    }

    protected virtual IList<VirtualSolutionFolder> GetVirtualSolutionFolders(XPathNavigator virtualSolutionFoldersNavigator)
    {
      var virtualSolutionFolders = new List<VirtualSolutionFolder>();
      foreach (XPathNavigator tokenNavigator in virtualSolutionFoldersNavigator.SelectChildren("virtualSolutionFolder", ""))
      {
        virtualSolutionFolders.Add(new VirtualSolutionFolder
        {
          Name = tokenNavigator.GetAttribute("name", ""),
          Files = GetPathValues(tokenNavigator).Select(cv => cv.Value).ToList(),
          SubFolders = GetVirtualSolutionFolders(tokenNavigator)
        });
      }
      return virtualSolutionFolders;
    }

    protected virtual IList<ConditionalValue> GetPathValues(XPathNavigator tokenNavigator, string attributeName = "file")
    {
      var paths = new List<ConditionalValue>();
      foreach (XPathNavigator navigator in tokenNavigator.SelectChildren(attributeName, ""))
      {
        var conditionValue = GetConditionalValue(navigator);
        if (string.IsNullOrEmpty(conditionValue.Value))
          throw new ManifestParseException("Missing path attribute on element.");
        paths.Add(conditionValue);
      }
      return paths;
    }

    protected virtual void ParseProjectsToAttach()
    {
      var projectFileNavigator = GetRequiredNodeByXPath("/templateManifest/projectsToAttach");
      foreach (XPathNavigator tokenNavigator in projectFileNavigator.SelectChildren("projectFile", ""))
      {
        var value = GetConditionalValue(tokenNavigator);
        if (string.IsNullOrEmpty(value.Value)) throw new ManifestParseException("Missing path attribute on project file to attach");
        Manifest.ProjectsToAttach.Add(value);
      }
    }

    protected ConditionalValue GetConditionalValue(XPathNavigator navigator)
    {
      var evaluatorType = navigator.GetAttribute("type", "");
      var path = navigator.GetAttribute("path", "");
      return new ConditionalValue
      {
        // TODO: if glob, Directory.EnumerateFiles search pattern
        Value = !path.StartsWith("*") ? GetFullPath(path) : path,
        Condition = navigator.GetAttribute("condition", ""),
        ConditionEvaluator = !string.IsNullOrEmpty(evaluatorType) ? 
          ManifestTypeInstantiator.CreateInstance<IEvaluateCondition>(evaluatorType) : DefaultConditionEvaluator
      };
    }

    protected virtual void ParseReplacementTokens()
    {
      var replacementTokensNavigator = GetRequiredNodeByXPath("/templateManifest/replacementTokens");
      ParseTokens(replacementTokensNavigator, Manifest.Tokens);
      foreach (XPathNavigator sectionNavigator in replacementTokensNavigator.SelectChildren("tokenSection", ""))
      {
        var section = new TokenSection
        {
          DisplayName = sectionNavigator.GetAttribute("displayName", ""),
          Description = sectionNavigator.GetAttribute("description", ""),
          Tokens = new List<ITokenDescription>()
        };
        if (string.IsNullOrWhiteSpace(section.DisplayName))
          section.DisplayName = "[Section]";
        ParseTokens(sectionNavigator, section.Tokens);
        Manifest.TokenSections.Add(section);
      }
    }

    protected virtual void ParseTokens(XPathNavigator xPathNavigator, IList<ITokenDescription> tokens)
    {
      foreach (XPathNavigator tokenNavigator in xPathNavigator.SelectChildren("token", ""))
      {
        var selectionOptions = GetTokenSelectOptions(tokenNavigator);
        var tokenInputType = selectionOptions.Any() ? TokenInputForm.Selection
          : Enum.TryParse(tokenNavigator.GetAttribute("input", ""), out TokenInputForm inputFormat) ? inputFormat : TokenInputForm.Text;

        var tokenDescription = new TokenDescription
        {
          DisplayName = tokenNavigator.GetAttribute("displayName", ""),
          HelpText = tokenNavigator.GetAttribute("helpText", ""),
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
        tokens.Add(tokenDescription);
      }
      ExpandDefaultValues(0, tokens);
    }

    protected virtual void ExpandDefaultValues(int noOfRuns, IList<ITokenDescription> tokens)
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

      if (!Manifest.Tokens.Any(t => t.Default.Contains("$")) || noOfRuns >= tokens.Count) return;
      noOfRuns++;

      ExpandDefaultValues(noOfRuns, tokens);
    }

    protected virtual KeyValuePair<string, string>[] GetTokenSelectOptions(XPathNavigator tokenNavigator)
    {
      var tokenSelectOptions = new Dictionary<string, string>();
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

    protected virtual bool GetBooleanAttribute(string attr, XPathNavigator navigator, bool defaultValue)
    {
      var value = navigator.GetAttribute(attr, "");
      if (string.IsNullOrEmpty(value)) return defaultValue;
      bool.TryParse(value, out defaultValue);
      return defaultValue;
    }

    protected virtual void ParseTemplateEngine()
    {
      var type = GetRequiredValue("/templateManifest/templateEngine/@type");
      Manifest.TemplateEngine = ManifestTypeInstantiator.CreateInstance<IHelixTemplateEngine>(type);
    }

    protected virtual void ParseManifestInformation()
    {
      Manifest.Name = GetRequiredValue("/templateManifest/name");
      Manifest.Description = GetRequiredValue("/templateManifest/description");
      Manifest.Link = GetHyperLink(GetNodeByXPath("/templateManifest/link"));
      Manifest.Version = GetRequiredValue("/templateManifest/version");
      Manifest.Author = GetRequiredValue("/templateManifest/author");
      Manifest.SourceFolder = GetFullPath(GetRequiredValue("/templateManifest/sourceFolder"), true);
      Manifest.SaveOnCreate = bool.TryParse(GetRequiredValue("/templateManifest/saveOnCreate"), out bool saveOnCreate) && saveOnCreate;
    }

    protected TemplateHyperLink GetHyperLink(XPathNavigator linkNode)
    {
      var linkUrl = linkNode?.Value;
      if (string.IsNullOrEmpty(linkUrl) || !Uri.IsWellFormedUriString(linkUrl, UriKind.Absolute)) return null;
      return new TemplateHyperLink { LinkText = linkNode.GetAttribute("text", ""), LinkUri = new Uri(linkUrl) };
    }

    protected string GetFullPath(string path, bool createFolder = false)
    {
      path = path.Replace("/", @"\");
      var fullPath = path.StartsWith(Manifest.ManifestRootPath) ? path : CombinePaths(Manifest.ManifestRootPath, path);
      if (File.Exists(fullPath)  || Directory.Exists(fullPath))
        return fullPath;
      
      if (createFolder)
      {
        return Directory.CreateDirectory(fullPath).FullName;
      }

      throw new ManifestParseException($"Could not find file or directory on relative {path} - expected file on full path {fullPath}");
    }

    protected string CombinePaths(string path1, string path2)
    {
      var pathParts = path2.Split('\\').Where(s => !string.IsNullOrEmpty(s)).ToList();
      if (path1[path1.Length - 1] == '\\')
        path1 = path1.Remove(path1.Length - 1);
      pathParts.Insert(0, path1);
      return string.Join(@"\", pathParts);
    }

    protected string GetRequiredValue(string xPath)
    {
      return GetRequiredNodeByXPath(xPath).Value;
    }

    protected XPathNavigator GetNodeByXPath(string xPath)
    {
      return GetNodeByXPath(xPath, RootNavigator);
    }

    protected XPathNavigator GetNodeByXPath(string xPath, XPathNavigator navigator)
    {
      return navigator.SelectSingleNode(xPath);
    }

    protected XPathNavigator GetRequiredNodeByXPath(string xPath)
    {
      var xPathNavigator = GetNodeByXPath(xPath);
      if (xPathNavigator != null) return xPathNavigator;
      throw new ManifestParseException(xPath);
    }
  }
}