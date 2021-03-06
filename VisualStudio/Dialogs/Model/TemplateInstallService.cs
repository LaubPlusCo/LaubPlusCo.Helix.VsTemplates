﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Properties;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model
{
  public class TemplateInstallService
  {
    private const string DefaultTemplateFolderName = "HelixTemplates";
    protected readonly IDictionary<TemplateType, DirectoryInfo[]> TemplateFolders;
    protected readonly string TemplatesRootPath;
    protected DirectoryInfo TempDirectory;

    public TemplateInstallService(string templatesRootPath, string zipFilePath = null)
    {
      TempDirectory = FileStorageService.Instance.TemporaryDirectory;
      TemplatesRootPath = templatesRootPath;
      TemplateFolders = new Dictionary<TemplateType, DirectoryInfo[]>();

      if (!string.IsNullOrEmpty(zipFilePath)
          && File.Exists(zipFilePath))
      {
        var extractedZipPath = ExtractTemplateZipFile(zipFilePath,
          Path.Combine(TempDirectory.FullName, "o"));

        TemplateFolders[TemplateType.Module] = GetTemplateDirectories(QuickLocateFolder(extractedZipPath, "modules"));
        TemplateFolders[TemplateType.Solution] =
          GetTemplateDirectories(QuickLocateFolder(extractedZipPath, "solutions"));
        return;
      }

      TemplateFolders[TemplateType.Module] = ExtractBuiltinZip(TemplateType.Module);
      TemplateFolders[TemplateType.Solution] = ExtractBuiltinZip(TemplateType.Solution);
    }

    protected string QuickLocateFolder(string path, string folderName)
    {
      if (Directory.Exists(Path.Combine(path, folderName)))
        return Path.Combine(path, folderName);
      foreach (var subFolder in Directory.GetDirectories(path))
        if (Directory.Exists(Path.Combine(subFolder, folderName)))
          return Path.Combine(subFolder, folderName);
      return "";
    }

    public static string CreateTemplateFolder(string rootPath, string folderName = DefaultTemplateFolderName)
    {
      if (!Directory.Exists(rootPath))
        throw new ArgumentException("Root path does not exist, cannot create template folder");
      var templateFolderPath = Path.Combine(rootPath, folderName);
      if (!Directory.Exists(templateFolderPath))
        Directory.CreateDirectory(templateFolderPath);
      return templateFolderPath;
    }

    public DirectoryInfo[] ExtractBuiltinZip(TemplateType templateType)
    {
      var tempFilePath = Path.Combine(TempDirectory.FullName, $"{templateType.ToString()}.zip");
      File.WriteAllBytes(tempFilePath,
        templateType == TemplateType.Module ? Resources.ModuleTemplates : Resources.SolutionTemplates);
      var extractPath = ExtractTemplateZipFile(tempFilePath,
        Path.Combine(TempDirectory.FullName, $"{templateType.ToString().Substring(0, 2)}"));
      return GetTemplateDirectories(extractPath);
    }

    protected string ExtractTemplateZipFile(string zipFilePath, string tempDirPath)
    {
      if (Directory.Exists(tempDirPath)) FileStorageService.Instance.Delete(tempDirPath);
      Directory.CreateDirectory(tempDirPath);
      ZipFile.ExtractToDirectory(zipFilePath, tempDirPath);
      return tempDirPath;
    }

    protected DirectoryInfo[] GetTemplateDirectories(string fromPath)
    {
      if (!Directory.Exists(fromPath))
        return new DirectoryInfo[0];
      return Directory.GetDirectories(fromPath)
        .Where(d => Directory.GetFiles(d).Any(f => f.Contains("template.manifest."))).Select(d => new DirectoryInfo(d))
        .ToArray();
    }

    public virtual bool Install()
    {
      return Install(TemplateType.Module)
             && Install(TemplateType.Solution);
    }

    public virtual bool Install(TemplateType templateType)
    {
      if (!TemplateFolders.ContainsKey(templateType))
        return false;
      foreach (var directoryInfo in TemplateFolders[templateType])
        CopyDirectory(directoryInfo, TemplatesRootPath);
      return true;
    }

    public static void CopyDirectory(DirectoryInfo sourceDir, string destinationRootPath)
    {
      //Note: This is to circumvent long path issues when running within Visual Studio.
      RunXCopy(sourceDir.FullName, Path.Combine(destinationRootPath, sourceDir.Name));
    }

    private static void RunXCopy(string sourceDirectory, string destinationDirectory)
    {
      var processInfo = new ProcessStartInfo
      {
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        FileName = "xcopy",
        WindowStyle = ProcessWindowStyle.Hidden,
        Arguments = $"\"{sourceDirectory}\" \"{destinationDirectory}\" /e /y /I"
      };
      StreamReader outputReader = null;
      try
      {
        using (var process = Process.Start(processInfo))
        {
          if (process == null)
            return;
          outputReader = process.StandardOutput;
          var waitCount = 0;
          while (!process.HasExited)
          {
            if (waitCount > 30)
            {
              WriteTraceService.WriteToTrace($"xcopy {sourceDirectory} to {destinationDirectory} locked:");
              process.Kill();
              break;
            }
            Thread.Sleep(100);
            waitCount++;
          }
          WriteTraceService.WriteToTrace($"Templates copied using xcopy:\n{outputReader.ReadToEnd()}");
        }
      }
      catch (Exception exception)
      {
        WriteTraceService.WriteToTrace(exception.Message, "Error");
      }
      finally
      {
        outputReader?.Close();
      }
    }
  }
}