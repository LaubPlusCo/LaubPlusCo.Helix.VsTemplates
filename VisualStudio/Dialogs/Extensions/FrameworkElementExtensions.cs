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

      root.SetElementStyle(depObj as TextBox, VsResourceKeys.TextBoxStyleKey);
      root.SetElementStyle(depObj as Button, VsResourceKeys.ThemedDialogButtonStyleKey);
      root.SetElementStyle(depObj as ListView, VsResourceKeys.ThemedDialogListViewStyleKey);
      root.SetElementStyle(depObj as ListViewItem, VsResourceKeys.ThemedDialogListViewItemStyleKey);
      root.SetElementStyle(depObj as ListBox, VsResourceKeys.ThemedDialogListBoxStyleKey);
      root.SetElementStyle(depObj as RadioButton, VsResourceKeys.ThemedDialogRadioButtonStyleKey);
      root.SetElementStyle(depObj as CheckBox, VsResourceKeys.ThemedDialogCheckBoxStyleKey);
      root.SetElementStyle(depObj as TreeView, VsResourceKeys.ThemedDialogTreeViewStyleKey);
      root.SetElementStyle(depObj as TreeViewItem, VsResourceKeys.ThemedDialogTreeViewItemStyleKey);
      root.SetElementStyle(depObj as Window, VsResourceKeys.ThemedDialogDefaultStylesKey);

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

    private static void SetElementStyle(this FrameworkElement root, FrameworkElement element, object resourceKey)
    {
      if (element == null || !(element.TryFindResource(resourceKey) is Style style))
        return;
      element.Style = style;
    }
  }
}