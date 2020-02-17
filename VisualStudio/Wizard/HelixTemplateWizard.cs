/* Sitecore Helix Visual Studio Templates 
 * 
 * Copyright (C) 2020, Anders Laub - Laub plus Co, DK 29 89 76 54 contact@laubplusco.net https://laubplusco.net
 * 
 * Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby granted, 
 * provided that the above copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING 
 * ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, 
 * DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, 
 * WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE 
 * OR PERFORMANCE OF THIS SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using EnvDTE;
using EnvDTE80;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;
using LaubPlusCo.VisualStudio.Helix.Wizard.Services;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.TemplateWizard;

namespace LaubPlusCo.VisualStudio.Helix.Wizard
{
  public class HelixTemplateWizard : IWizard
  {
    private string _destinationDirectory;
    private DTE2 _dte;
    private bool? _isExclusive;
    private ManifestDialog _manifestBrowseDialog;
    private IHelixProjectTemplate _projectTemplate;
    private Dictionary<string, string> _replacementTokens;
    private string _solutionRootDirectory;

    public void RunStarted(object automationObject, Dictionary<string, string> replacementTokens, WizardRunKind runKind,
      object[] customParams)
    {
      if (!AppScopeSettings.Current.VersionNoticeShown)
      {
        var versionMessageDialog = new VersionMessageDialog();
        versionMessageDialog.ShowDialog();
      }

      _dte = automationObject as DTE2;
      _replacementTokens = replacementTokens;
      _destinationDirectory = replacementTokens["$destinationdirectory$"];
      _solutionRootDirectory = replacementTokens["$solutiondirectory$"];
      _isExclusive = replacementTokens["$exclusiveproject$"] == null ? null :
        bool.TryParse(replacementTokens["$exclusiveproject$"], out var b) ? (bool?) b : null;
      if (string.IsNullOrEmpty(_solutionRootDirectory)) _solutionRootDirectory = _destinationDirectory;

      if (AppScopeSettings.Current.IsFirstRun)
        ShowInitSetupDialog();

      ShowManifestDialog();
    }

    public bool ShouldAddProjectItem(string filePath)
    {
      return true;
    }

    public void RunFinished()
    {
      DeleteAutoCreatedDirectory();
      var attachToVisualStudioService = new AttachToVisualStudioService(_dte);
      attachToVisualStudioService.Attach(_projectTemplate);
      if (_projectTemplate.Manifest.SaveOnCreate)
        SaveAll();
      FocusOnTraceWindow();
    }

    public void BeforeOpeningFile(ProjectItem projectItem)
    {
    }

    public void ProjectItemFinishedGenerating(ProjectItem projectItem)
    {
    }

    public void ProjectFinishedGenerating(Project project)
    {
    }

    private static bool IsAdministrator()
    {
      return new WindowsPrincipal(WindowsIdentity.GetCurrent())
        .IsInRole(WindowsBuiltInRole.Administrator);
    }

    private void ShowInitSetupDialog()
    {
      try
      {
        var settingsDialog = new SettingsDialog();
        var settingsDialogResult = settingsDialog.ShowDialog();
        if (!settingsDialogResult.HasValue || !settingsDialogResult.Value)
          throw new WizardBackoutException();
      }
      catch (Exception exception)
      {
        if (!(exception is WizardBackoutException))
          Trace.WriteLine($"{exception.Message}\n\n{exception.StackTrace}", "Error");
        FocusOnTraceWindow();
        DeleteAutoCreatedDirectory();
        throw;
      }
    }

    private void ShowManifestDialog()
    {
      try
      {
        _projectTemplate = GetHelixProjectTemplate(_solutionRootDirectory);
        if (_projectTemplate == null)
          throw new WizardBackoutException();
      }
      catch (Exception exception)
      {
        if (!(exception is WizardBackoutException))
          Trace.WriteLine($"Exception occurred: {exception.Message}\n\n{exception.StackTrace}", "Error");
        FocusOnTraceWindow();
        DeleteAutoCreatedDirectory();
        throw;
      }
    }

    private IHelixProjectTemplate GetHelixProjectTemplate(string solutionRootDirectory)
    {
      _manifestBrowseDialog = new ManifestDialog();
      _manifestBrowseDialog.Initialize(AppScopeSettings.Current.TemplatesFolder, solutionRootDirectory,
        _replacementTokens, _isExclusive.HasValue && _isExclusive.Value);
      var dialogResult = _manifestBrowseDialog.ShowDialog();
      if (dialogResult.HasValue && dialogResult.Value)
        return _manifestBrowseDialog.HelixProjectTemplate;
      return null;
    }

    private void FocusOnTraceWindow()
    {
      if (_manifestBrowseDialog?.TraceWindow == null || !_manifestBrowseDialog.TraceWindow.IsVisible)
        return;
      _manifestBrowseDialog.TraceWindow.BringIntoView();
      _manifestBrowseDialog.TraceWindow.Focus();
    }

    private void SaveAll()
    {
      _dte.ExecuteCommand("File.SaveAll");
    }

    private void DeleteAutoCreatedDirectory()
    {
      if (!Directory.Exists(_destinationDirectory)) return;
      Directory.Delete(_destinationDirectory, true);
    }
  }
}