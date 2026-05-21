# Carousel Control for Uno Platform

A full-featured carousel control built for Uno Platform with looping, indicators, transitions, swipe gestures, auto-play, and accessibility support.

<!-- Screenshot placeholder: add a GIF or screenshot here -->

## Features

- Horizontal and vertical orientation
- Infinite looping (optional)
- Built-in slide and fade transitions
- Custom transitions via `ICarouselTransition`
- Touch/swipe gesture navigation with peek and snap-back
- Page indicator dots (theme-aware)
- Auto-play with configurable interval (pauses on hover)
- Keyboard navigation (Arrow keys, Home, End)
- Full accessibility via `CarouselAutomationPeer`
- Two-way `SelectedIndex` binding
- Cancellable `SelectionChanging` event

## Public API

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `ItemsSource` | `object` | `null` | Collection of items to display |
| `ItemTemplate` | `DataTemplate` | `null` | Template for rendering each item |
| `SelectedIndex` | `int` | `-1` | Index of the currently visible item (two-way bindable) |
| `SelectedItem` | `object` | `null` | The currently visible data item |
| `Orientation` | `Orientation` | `Horizontal` | Layout direction (Horizontal or Vertical) |
| `IsLoopingEnabled` | `bool` | `true` | Whether navigation wraps from last to first |
| `IsAutoPlayEnabled` | `bool` | `false` | Whether auto-advance is active |
| `AutoPlayInterval` | `TimeSpan?` | `null` | Interval between auto-advance steps |
| `ItemTransition` | `ICarouselTransition` | `SlideTransition` | Pluggable transition animation |
| `IndicatorVisibility` | `Visibility` | `Visible` | Show or hide the page indicator dots |
| `IndicatorStyle` | `Style` | `null` | Custom style for indicator dots |

### Events

| Event | Args | Description |
|---|---|---|
| `SelectionChanged` | `CarouselSelectionChangedEventArgs` | Fires after the selected item changes |
| `SelectionChanging` | `CarouselSelectionChangingEventArgs` | Fires before selection changes; set `Cancel = true` to prevent |

### Methods

| Method | Description |
|---|---|
| `Next()` | Advance to the next item |
| `Previous()` | Go back to the previous item |
| `GoTo(int index, bool animate = true)` | Navigate to a specific index |

## Built-in Transitions

- **SlideTransition** (default) - Items slide in/out horizontally or vertically
- **FadeTransition** - Cross-fade between items

### Custom Transitions

Implement `ICarouselTransition`:

```csharp
public interface ICarouselTransition
{
    void Run(UIElement? from, UIElement to, bool forward, Action onCompleted);
    void Cancel();
}
```

## XAML Usage

```xml
<utu:Carousel x:Name="MyCarousel"
              Height="400"
              CornerRadius="12"
              IsLoopingEnabled="True"
              IsAutoPlayEnabled="True"
              AutoPlayInterval="0:0:3">
    <utu:Carousel.ItemTemplate>
        <DataTemplate>
            <Image Source="{Binding ImageUrl}"
                   Stretch="UniformToFill" />
        </DataTemplate>
    </utu:Carousel.ItemTemplate>
</utu:Carousel>
```

```csharp
MyCarousel.ItemsSource = new List<PhotoItem>
{
    new("https://example.com/photo1.jpg", "Photo 1"),
    new("https://example.com/photo2.jpg", "Photo 2"),
};
```

## Platform Support

| Platform | Status |
|---|---|
| Desktop (Skia) | Supported |
| WebAssembly | Supported |
| Android | Supported |

## Project Structure

```
Controls/Carousel/
  Carousel.cs              - Core logic, navigation, swipe gestures
  Carousel.Properties.cs   - Dependency properties
  Carousel.Events.cs       - Event args and event declarations
  Carousel.xaml            - Control template and theme resources
  CarouselPanel.cs         - Overlay panel (stacks items at 0,0)
  CarouselIndicator.cs     - Page indicator dots
  CarouselAutomationPeer.cs- Accessibility support
  ICarouselTransition.cs   - Transition interface
  SlideTransition.cs       - Slide animation
  FadeTransition.cs        - Fade animation
```
