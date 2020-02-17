using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class LegacySettings
  {
    private const string RegistryKeyName = @"Software\LaubPlusCo\SitecoreHelixVsTemplates";
    private const string RootDirectoryObjectName = @"TemplatesRootDirectory";

    public static string GetGlobalTemplateFolder()
    {
      return GetRegistryKey(RootDirectoryObjectName);
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
  }
}