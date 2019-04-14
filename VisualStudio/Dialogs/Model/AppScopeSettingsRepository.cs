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
    private const string LoggingEnabledObjectName = @"LogEnabled";
    public const string DefaultRootPath = @"c:\projects\helix.templates\";

    public static string GetGlobalRootDirectory()
    {
      return GetRegistryKey(RootDirectoryObjectName);
    }

    public static bool SetGlobalRootDirectory(string rootDirectory)
    {
      return Directory.Exists(rootDirectory) && SetRegistryKey(RootDirectoryObjectName, rootDirectory);
    }

    public static bool GetLoggingEnabled()
    {
      return bool.TryParse(GetRegistryKey(LoggingEnabledObjectName), out var b) && b;
    }

    public static bool SetLoggingEnabled(bool enabled)
    {
      return SetRegistryKey(LoggingEnabledObjectName, enabled.ToString());
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
      catch (Exception)
      {
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
            Debug.Print("Cannot access or modify registry for storing template root directory");
            throw new SecurityException("Cannot access or modify registry for storing template root directory");
          }

          key.SetValue(objectName, value);
          return true;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}