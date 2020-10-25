using System.Collections.Generic;
using System.IO;
using System.Linq;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class ReadAllManifestFilesService
  {
    protected readonly IDictionary<string, string> ReplacementTokens;
    protected readonly string RootDirectory;

    public ReadAllManifestFilesService(string rootDirectory, IDictionary<string, string> replacementTokens)
    {
      RootDirectory = rootDirectory;
      ReplacementTokens = replacementTokens;
      if (!ReplacementTokens.ContainsKey("$emptyString$"))
        ReplacementTokens.Add("$emptyString$", string.Empty);
    }

    public HelixTemplateManifest[] Read()
    {
      var manifestFileInfos = FindManifestFiles();
      return !manifestFileInfos.Any() ? new HelixTemplateManifest[0] : ReadManifestFiles(manifestFileInfos).ToArray();
    }

    private IEnumerable<HelixTemplateManifest> ReadManifestFiles(FileInfo[] manifestFileInfos)
    {
      return manifestFileInfos.Select(fileInfo => new ParseManifestService(fileInfo.FullName).Parse(ReplacementTokens)).Where(m => m != null);
    }

    private FileInfo[] FindManifestFiles()
    {
      return new DirectoryInfo(RootDirectory).EnumerateFiles("template.manifest.xml", SearchOption.AllDirectories).ToArray();
    }
  }
}