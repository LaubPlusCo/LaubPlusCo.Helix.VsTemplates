using System.Collections.Generic;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;

namespace LaubPlusCo.Foundation.HelixTemplating.TemplateEngine
{
  public interface IHelixTemplateEngine
  {
    IHelixProjectTemplate Run(HelixTemplateManifest manifest, string solutionRootPath);
  }
}