using System.Collections.Generic;
using System.IO;
using LaubPlusCo.Foundation.HelixTemplating.Data;

namespace LaubPlusCo.Foundation.HelixTemplating.TemplateEngine
{
  public class TemplateObject : ITemplateObject
  {
    public TemplateObject()
    {
      ChildObjects = new List<ITemplateObject>();
    }
    public string OriginalFullPath { get; set; }
    public string DestinationFullPath { get; set; }
    public TemplateObjectType Type { get; set; }
    public bool IsIgnored { get; set; }
    public IList<ITemplateObject> ChildObjects { get; set; }
    public bool IsCreated { get; set; }
    public bool SkipAttach { get; set; }
    public string Name => Type == TemplateObjectType.Folder || Type == TemplateObjectType.SourceRoot
      ? new DirectoryInfo(DestinationFullPath).Name : Path.GetFileNameWithoutExtension(DestinationFullPath);
  }
}
