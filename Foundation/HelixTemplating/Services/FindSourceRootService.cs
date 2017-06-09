using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaubPlusCo.Foundation.HelixTemplating.Data;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class FindSourceRootTemplateObjectService
  {
    public static ITemplateObject Find(IList<ITemplateObject> templateObjects)
    {
      var sourceRoot = templateObjects.FirstOrDefault(templateObject => templateObject.Type == TemplateObjectType.SourceRoot);
      sourceRoot =  sourceRoot ?? templateObjects.Where(to => to.ChildObjects != null && to.ChildObjects.Any()).Select(templateObject => Find(templateObject.ChildObjects)).FirstOrDefault();
      return sourceRoot ?? templateObjects.FirstOrDefault(t => t.Type == TemplateObjectType.Folder);
    }
  }
}
