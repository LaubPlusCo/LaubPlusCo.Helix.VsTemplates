
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Sitecore.Framework.Runtime.Configuration;
using Sitecore.Framework.Runtime.Plugins;

namespace $moduleNamespace$
{
  public class ConfigureSitecore
  {
    public ConfigureSitecore(ILogger<ConfigureSitecore> logger, ISitecoreConfiguration configuration)
    {
    }

    public void ConfigureServices(IServiceCollection services)
    {
    }
  }
}