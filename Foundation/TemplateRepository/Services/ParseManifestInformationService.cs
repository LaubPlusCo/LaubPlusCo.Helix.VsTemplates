using System.IO;
using System.Xml;
using System.Xml.XPath;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;

namespace LaubPlusCo.Foundation.Helix.TemplateRepository.Services
{
  public class ParseManifestInformationService : ParseManifestService
  {
    public ParseManifestInformationService(string manifestFilePath) : base(manifestFilePath)
    {
    }

    public HelixTemplateManifest Parse()
    {
      Manifest = new HelixTemplateManifest(ManifestFilePath);
      var xmlDocumentdoc = new XPathDocument(XmlReader.Create(new StringReader(File.ReadAllText(ManifestFilePath)),
        new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Fragment}));
      RootNavigator = xmlDocumentdoc.CreateNavigator();
      ParseManifestInformation();
      return Manifest;
    }
  }
}