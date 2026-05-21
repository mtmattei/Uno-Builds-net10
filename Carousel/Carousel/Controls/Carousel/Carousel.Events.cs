using System;
using Windows.Foundation;

namespace Uno.Toolkit.UI;

public class CarouselSelectionChangedEventArgs(object? previousItem, object? newItem, int previousIndex, int newIndex) : EventArgs
{
    public object? PreviousItem { get; } = previousItem;
    public object? NewItem { get; } = newItem;
    public int PreviousIndex { get; } = previousIndex;
    public int NewIndex { get; } = newIndex;
}

public class CarouselSelectionChangingEventArgs(object? previousItem, object? newItem) : EventArgs
{
    public object? PreviousItem { get; } = previousItem;
    public object? NewItem { get; } = newItem;
    public bool Cancel { get; set; }
}

public partial class Carousel
{
    public event TypedEventHandler<Carousel, CarouselSelectionChangedEventArgs>? SelectionChanged;
    public event TypedEventHandler<Carousel, CarouselSelectionChangingEventArgs>? SelectionChanging;
}
