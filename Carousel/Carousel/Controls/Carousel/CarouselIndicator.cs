using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace Uno.Toolkit.UI;

internal partial class CarouselIndicator : Panel
{
    private int _itemCount;
    private int _selectedIndex;
    private Brush? _activeBrush;
    private Brush? _inactiveBrush;
    private double _dotSize = 8;
    private double _dotSpacing = 8;
    private Orientation _orientation = Orientation.Horizontal;

    private static readonly Brush DefaultActiveBrush = new SolidColorBrush(Colors.White);
    private static readonly Brush DefaultInactiveBrush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));

    private Brush EffectiveActiveBrush => _activeBrush ?? DefaultActiveBrush;
    private Brush EffectiveInactiveBrush => _inactiveBrush ?? DefaultInactiveBrush;

    internal int ItemCount
    {
        get => _itemCount;
        set { if (_itemCount != value) { _itemCount = value; RebuildDots(); } }
    }

    internal int SelectedIndex
    {
        get => _selectedIndex;
        set { if (_selectedIndex != value) { _selectedIndex = value; UpdateDotStates(); } }
    }

    internal Brush? ActiveBrush
    {
        get => _activeBrush;
        set { _activeBrush = value; UpdateDotStates(); }
    }

    internal Brush? InactiveBrush
    {
        get => _inactiveBrush;
        set { _inactiveBrush = value; UpdateDotStates(); }
    }

    internal double DotSize
    {
        get => _dotSize;
        set { _dotSize = value; RebuildDots(); }
    }

    internal double DotSpacing
    {
        get => _dotSpacing;
        set { _dotSpacing = value; InvalidateMeasure(); }
    }

    internal Orientation Orientation
    {
        get => _orientation;
        set { if (_orientation != value) { _orientation = value; InvalidateMeasure(); } }
    }

    private void RebuildDots()
    {
        Children.Clear();
        for (int i = 0; i < _itemCount; i++)
        {
            var active = i == _selectedIndex;
            Children.Add(new Ellipse
            {
                Width = _dotSize, Height = _dotSize,
                Fill = active ? EffectiveActiveBrush : EffectiveInactiveBrush,
                Opacity = active ? 1.0 : 0.5
            });
        }
        InvalidateMeasure();
    }

    private void UpdateDotStates()
    {
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] is Ellipse dot)
            {
                var active = i == _selectedIndex;
                dot.Fill = active ? EffectiveActiveBrush : EffectiveInactiveBrush;
                dot.Opacity = active ? 1.0 : 0.5;
            }
        }
    }

    // All dots are identical size, so we pre-compute totals instead of measuring each child.
    protected override Size MeasureOverride(Size availableSize)
    {
        var count = Children.Count;
        if (count == 0) return default;

        // Still need to call Measure on children for the layout system
        foreach (var child in Children)
            child.Measure(new Size(_dotSize, _dotSize));

        var totalMain = count * _dotSize + Math.Max(0, count - 1) * _dotSpacing;
        return _orientation == Orientation.Horizontal
            ? new Size(totalMain, _dotSize)
            : new Size(_dotSize, totalMain);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var count = Children.Count;
        if (count == 0) return finalSize;

        var totalMain = count * _dotSize + Math.Max(0, count - 1) * _dotSpacing;
        var horizontal = _orientation == Orientation.Horizontal;
        var offset = Math.Max(0, ((horizontal ? finalSize.Width : finalSize.Height) - totalMain) / 2.0);

        foreach (var child in Children)
        {
            if (horizontal)
            {
                var y = (finalSize.Height - _dotSize) / 2.0;
                child.Arrange(new Rect(offset, y, _dotSize, _dotSize));
            }
            else
            {
                var x = (finalSize.Width - _dotSize) / 2.0;
                child.Arrange(new Rect(x, offset, _dotSize, _dotSize));
            }
            offset += _dotSize + _dotSpacing;
        }
        return finalSize;
    }
}
