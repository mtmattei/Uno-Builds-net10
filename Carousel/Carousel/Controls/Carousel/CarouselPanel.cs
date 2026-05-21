using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Uno.Toolkit.UI;

/// <summary>
/// Custom panel that stacks all carousel items at the same position (0,0).
/// Visibility is controlled entirely by the parent Carousel via opacity
/// and Canvas.ZIndex - the panel never touches those properties.
/// </summary>
internal partial class CarouselPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var childSize = new Size(
            double.IsInfinity(availableSize.Width) ? 800 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 600 : availableSize.Height);

        foreach (var child in Children)
        {
            child.Measure(childSize);
        }

        return childSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = new Rect(0, 0, finalSize.Width, finalSize.Height);

        foreach (var child in Children)
        {
            child.Arrange(rect);
        }

        return finalSize;
    }
}
