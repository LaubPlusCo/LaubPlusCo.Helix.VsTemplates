/* Sitecore Helix Visual Studio Templates 
 * 
 * Copyright (C) 2017, Anders Laub - Laub plus Co, DK 29 89 76 54 contact@laubplusco.net https://laubplusco.net
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
using System.IO;
using EnvDTE;
using EnvDTE80;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;
using LaubPlusCo.VisualStudio.Helix.Wizard.Services;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.ManifestDialog;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.TemplateWizard;

namespace LaubPlusCo.VisualStudio.Helix.Wizard
{
  public class HelixTemplateWizard : IWizard
  {
    private string _destinationDirectory;
    private DTE2 _dte;
    private IHelixProjectTemplate _projectTemplate;
    private string _solutionRootDirectory;
    private Dictionary<string, string> _replacementTokens;
    private bool? _isExclusive;
    public void RunStarted(object automationObject, Dictionary<string, string> replacementTokens, WizardRunKind runKind, object[] customParams)
    {
      _dte = automationObject as DTE2;
      _replacementTokens = replacementTokens;
      _destinationDirectory = replacementTokens["$destinationdirectory$"];
      _solutionRootDirectory = replacementTokens["$solutiondirectory$"];
      _isExclusive = replacementTokens["$exclusiveproject$"] == null ? null : (bool.TryParse(replacementTokens["$exclusiveproject$"], out bool b) ? (bool?) b : null);
      if (string.IsNullOrEmpty(_solutionRootDirectory)) _solutionRootDirectory = _destinationDirectory;

      if (IsFirstRun)
        ShowInitSetupDialog();

      ShowManifestDialog();
    }

    public bool IsFirstRun => string.IsNullOrEmpty(TemplatesRootDirectoryPathRepository.Get()) || !Directory.Exists(TemplatesRootDirectoryPathRepository.Get());

    private void ShowInitSetupDialog()
    {
      try
      {
        var settingsDialog = new SettingsDialog();
        var settingsDialogResult = settingsDialog.ShowDialog();
        if (!settingsDialogResult.HasValue || !settingsDialogResult.Value)
          throw new WizardBackoutException();
      }
      catch (Exception)
      {
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
      catch (Exception)
      {
        DeleteAutoCreatedDirectory();
        throw;
      }
    }

    private IHelixProjectTemplate GetHelixProjectTemplate(string solutionRootDirectory)
    {
      var manifestBrowseDialog = new ManifestDialog();
      manifestBrowseDialog.Initialize(TemplatesRootDirectoryPathRepository.Get(), solutionRootDirectory, _replacementTokens, _isExclusive);
      var dialogResult = manifestBrowseDialog.ShowDialog();
      if (dialogResult.HasValue && dialogResult.Value)
        return manifestBrowseDialog.HelixProjectTemplate;
      return null;
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
    }

    private void SaveAll()
    {
      _dte.ExecuteCommand("File.SaveAll");
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

    private void DeleteAutoCreatedDirectory()
    {
      if (!Directory.Exists(_destinationDirectory)) return;
      Directory.Delete(_destinationDirectory, true);
    }
  }
}