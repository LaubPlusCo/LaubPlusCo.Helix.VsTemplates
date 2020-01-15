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
using System.IO;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.Foundation.HelixTemplating.TemplateEngine;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public class HelixTemplateManifest
  {
    public string ManifestFileName;

    public HelixTemplateManifest(string manifestFile)
    {
      var manifestFileInfo = new FileInfo(manifestFile);
      ManifestFileName = manifestFileInfo.Name;
      ManifestRootPath = manifestFileInfo.Directory?.FullName;
      Tokens = new List<TokenDescription>();
      ProjectsToAttach = new List<string>();
      IgnoreFiles = new List<string>();
      SkipAttachPaths = new List<string>();
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public TemplateHyperLink Link { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string ManifestRootPath { get; set; }
    public string SourceFolder { get; set; }
    public bool SaveOnCreate { get; set; }
    public IHelixTemplateEngine TemplateEngine { get; set; }
    public IList<TokenDescription> Tokens { get; set; }
    public IList<string> ProjectsToAttach { get; set; }
    public IList<string> SkipAttachPaths { get; set; }
    public IList<string> IgnoreFiles { get; set; }
    public IList<VirtualSolutionFolder> VirtualSolutionFolders { get; set; }
    public TemplateType TemplateType { get; set; }
    public IDictionary<string,string> ReplacementTokens { get; set; }
  }

  public class TemplateHyperLink
  {
    public Uri LinkUri { get; set; }
    public string LinkText { get; set; }
  }
}