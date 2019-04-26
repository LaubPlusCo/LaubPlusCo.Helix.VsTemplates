using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using Microsoft.Win32;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class AppScopeSettingsRepository
  {
    private const string RegistryKeyName = @"Software\LaubPlusCo\SitecoreHelixVsTemplates";
    private const string RootDirectoryObjectName = @"TemplatesRootDirectory";
    public const string DefaultRootPath = @"c:\projects\helix.templates\";

    public static string GetGlobalRootDirectory()
    {
      return GetRegistryKey(RootDirectoryObjectName);
    }

    public static bool SetGlobalRootDirectory(string rootDirectory)
    {
      return Directory.Exists(rootDirectory) && SetRegistryKey(RootDirectoryObjectName, rootDirectory);
    }

    private static string GetRegistryKey(string objectName)
    {
      try
      {
        using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyName))
        {
          if (key == null) return string.Empty;
          return (string) key.GetValue(objectName, string.Empty);
        }
      }
      catch (Exception exception)
      {
        Trace.WriteLine($"Exception while reading registry e\n{exception.Message}\n{exception.StackTrace}", "Error");
        return string.Empty;
      }
    }

    private static bool SetRegistryKey(string objectName, string value)
    {
      try
      {
        using (var key =
          Registry.CurrentUser.OpenSubKey(RegistryKeyName, RegistryKeyPermissionCheck.ReadWriteSubTree,
            RegistryRights.WriteKey) ?? Registry.CurrentUser.CreateSubKey(RegistryKeyName))
        {
          if (key == null)
          {
            Trace.WriteLine("Cannot access or modify registry for storing template root directory", "Error");
            throw new SecurityException("Cannot access or modify registry for storing template root directory");
          }

          key.SetValue(objectName, value);
          return true;
        }
      }
      catch (Exception exception)
      {
        Trace.WriteLine($"Exception while setting registry key value\n{exception.Message}\n{exception.StackTrace}",
          "Error");
        return false;
      }
    }
  }
}