using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace LaubPlusCo.VisualStudio.HelixTemplates.Dialogs.Dialogs
{
  public class TokenSectionTabItem : TabItem
  {
    public readonly Label HeaderLabel;
    public readonly StackPanel InnerPanel;
    protected readonly ScrollViewer ScrollViewer;

    public TokenSectionTabItem(string header)
    {
      ScrollViewer = new ScrollViewer
      {
        CanContentScroll = true,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
      };

      InnerPanel = new StackPanel
      {
        ScrollOwner = ScrollViewer,
        CanVerticallyScroll = true,
        CanHorizontallyScroll = false,
        Margin = new Thickness(20),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        MaxHeight = 450
      };

      HeaderLabel = new Label
      {
        Content = header,
        Foreground = (Brush) FindResource(VsBrushes.CaptionTextKey),
        FontFamily = (FontFamily) FindResource(VsFonts.EnvironmentFontFamilyKey),
        Style = (Style) FindResource(VsResourceKeys.LabelEnvironment133PercentFontSizeStyleKey)
      };

      Initialize();
    }

    protected void Initialize()
    {
      ScrollViewer.Content = InnerPanel;
      AddChild(ScrollViewer);
      Header = HeaderLabel;
    }
  }
}