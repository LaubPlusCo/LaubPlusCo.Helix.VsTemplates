using System;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.ManifestDialog
{
  public class TokenValueChangedArgs : EventArgs
  {
    public string Value { get; set; }
    public string Key { get; set; }
  }
}