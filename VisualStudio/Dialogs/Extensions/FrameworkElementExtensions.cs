using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Extensions
{
  public static class FrameworkElementExtensions
  {
    public static void SetVisualStudioThemeStyles(this FrameworkElement root)
    {
      IterateLogicalTree(root);
    }

    internal static void IterateLogicalTree(FrameworkElement root, object current = null)
    {
      current = current ?? root;

      if (!(current is DependencyObject depObj)) return;

      if (Equals(current, root)) root.SetRootStyles();

      SetElementStyle(depObj as TextBox, VsResourceKeys.TextBoxStyleKey);
      SetElementStyle(depObj as Button, VsResourceKeys.ThemedDialogButtonStyleKey);
      SetElementStyle(depObj as ListView, VsResourceKeys.ThemedDialogListViewStyleKey);
      SetElementStyle(depObj as ListViewItem, VsResourceKeys.ThemedDialogListViewItemStyleKey);
      SetElementStyle(depObj as ListBox, VsResourceKeys.ThemedDialogListBoxStyleKey);
      SetElementStyle(depObj as RadioButton, VsResourceKeys.ThemedDialogRadioButtonStyleKey);
      SetElementStyle(depObj as CheckBox, VsResourceKeys.ThemedDialogCheckBoxStyleKey);
      SetElementStyle(depObj as TreeView, VsResourceKeys.ThemedDialogTreeViewStyleKey);
      SetElementStyle(depObj as TreeViewItem, VsResourceKeys.ThemedDialogTreeViewItemStyleKey);

      if (depObj is Control control)
      {
        control.FontFamily = (FontFamily)control.TryFindResource(VsFonts.EnvironmentFontFamilyKey);
      }
      if (depObj is TextBlock textBlock)
      {
        textBlock.FontFamily = (FontFamily)textBlock.TryFindResource(VsFonts.EnvironmentFontFamilyKey);
      }

      foreach (var logicalChild in LogicalTreeHelper.GetChildren(depObj)) IterateLogicalTree(root, logicalChild);
    }

    private static void SetRootStyles(this FrameworkElement root)
    {
      if (!(root is Window window)
          || !(root.TryFindResource(VsBrushes.ToolWindowBackgroundKey) is Brush background)) return;
      window.Background = background;
    }

    public static void SetElementStyle(FrameworkElement element, object resourceKey)
    {
      if (element == null || !(element.TryFindResource(resourceKey) is Style style))
        return;
      element.Style = style;
    }
  }
}