using System.Text.RegularExpressions;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class BuildDestinationPathService
  {
    private readonly string _destinationRoot;
    private readonly string _originalRoot;

    public BuildDestinationPathService(string originalRoot, string destinationRoot)
    {
      _originalRoot =  originalRoot.ToLowerInvariant().TrimEnd('\\', ' ').Replace(@"\", "/");
      _destinationRoot = destinationRoot.ToLowerInvariant().TrimEnd('\\', ' ').Replace(@"\", "/");
    }

    public string Build(string originalFullPath)
    {
      originalFullPath = originalFullPath.Replace(@"\", "/");
      return Regex.Unescape(Regex.Replace(originalFullPath, Regex.Escape(_originalRoot), Regex.Escape(_destinationRoot), RegexOptions.IgnoreCase)).Replace("/", "\\");
    }
  }
}