using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using LaubPlusCo.Foundation.HelixTemplating.Manifest;
using LaubPlusCo.Foundation.HelixTemplating.Services;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions;
using LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Model;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs.Controls
{
  /// <summary>
  ///   Interaction logic for ExploreTemplates.xaml
  /// </summary>
  public partial class ExploreTemplates : UserControl
  {
    public ExploreTemplates()
    {
      InitializeComponent();
    }
  }
}