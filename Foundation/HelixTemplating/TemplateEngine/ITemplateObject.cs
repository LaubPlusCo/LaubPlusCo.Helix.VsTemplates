using System.Collections.Generic;
using LaubPlusCo.Foundation.HelixTemplating.Data;

namespace LaubPlusCo.Foundation.HelixTemplating.TemplateEngine
{
  public interface ITemplateObject
  {
    string OriginalFullPath { get; set; }
    string DestinationFullPath { get; set; }
    TemplateObjectType Type { get; set; }
    bool IsIgnored { get; set; }
    IList<ITemplateObject> ChildObjects { get; set; }
    bool IsCreated { get; set; }
    bool IsProjectContent { get; set; }
    string Name { get; }
  }
}