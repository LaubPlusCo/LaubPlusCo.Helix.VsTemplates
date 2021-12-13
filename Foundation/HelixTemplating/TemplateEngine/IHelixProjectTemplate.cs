﻿/* Sitecore Helix Visual Studio Templates 
 * 
 * Copyright (C) 2021, Anders Laub - Laub plus Co, DK 29 89 76 54 contact@laubplusco.net https://laubplusco.net
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

using System.Collections.Generic;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;

namespace LaubPlusCo.Foundation.HelixTemplating.TemplateEngine
{
  public interface IHelixProjectTemplate
  {
    HelixTemplateManifest Manifest { get; set; }
    IList<ITemplateObject> TemplateObjects { get; set; }
    IDictionary<string, string> ReplacementTokens { get; set; }
  }
}