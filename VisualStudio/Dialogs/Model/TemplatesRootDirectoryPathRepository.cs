using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class TemplatesRootDirectoryPathRepository
  {
    private const string RegistryKeyName = @"Software\LaubPlusCo\SitecoreHelixVsTemplates";
    private const string RegistryObjectName = @"TemplatesRootDirectory";
    public const string DefaultRootPath = @"c:\projects\helix.templates\";

    public static string Get()
    {
      try
      {
        using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyName))
        {
          if (key == null) return string.Empty;
          return (string)key.GetValue(RegistryObjectName, string.Empty);
        }
      }
      catch (Exception exception)
      {
        return string.Empty;
      }
    }

    public static bool Set(string rootDirectory)
    {
      if (!Directory.Exists(rootDirectory))
        return false;
      try
      {
        using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.WriteKey) ?? Registry.CurrentUser.CreateSubKey(RegistryKeyName))
        {
          if (key == null) throw new SecurityException("Cannot access or modify registry for storing template root directory");
          key.SetValue(RegistryObjectName, rootDirectory);
          return true;
        }
      }
      catch (Exception exception)
      {
        return false;
      }
    }
  }
}
