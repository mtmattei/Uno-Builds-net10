---
uid: Toolkit.Controls.Snackbar
---

# Snackbar

## Summary

`Snackbar` and `SnackbarHost` provide brief messages about app processes at the bottom of the screen, following the [Material Design 3 Snackbar specification](https://m3.material.io/components/snackbar/specs). Messages appear temporarily and can include an optional action button and dismiss button.

### C# / Code-behind

```csharp
// 1. Get a reference to the SnackbarHost
var host = this.FindName("MySnackbarHost") as SnackbarHost;

// 2. Show a simple message (auto-dismisses after 4s)
var reason = await host.ShowAsync(new SnackbarItem
{
    Message = "Photo saved to gallery"
});

// 3. Show with action button
var reason = await host.ShowAsync(new SnackbarItem
{
    Message = "Item deleted",
    ActionLabel = "Undo",
    ActionCommand = new RelayCommand(() => UndoDelete()),
});

if (reason == SnackbarDismissReason.Action)
{
    // User tapped the action button
}

// 4. Multi-line with action on new line + dismiss button
await host.ShowAsync(new SnackbarItem
{
    Message = "This item already has the label. You can add a new one.",
    ActionLabel = "Add label",
    ShowDismissButton = true,
    IsActionOnNewLine = true,
    Duration = SnackbarDuration.Long,
});
```

### XAML

```xml
<Page xmlns:utu="using:Uno.Toolkit.UI">

    <utu:SnackbarHost x:Name="MySnackbarHost">
        <!-- Your page content goes here -->
        <Grid>
            <TextBlock Text="Main content" />
        </Grid>
    </utu:SnackbarHost>

</Page>
```

## M3 Specification Compliance

This implementation follows the [Material Design 3 Snackbar specs](https://m3.material.io/components/snackbar/specs). The table below maps each M3 requirement to the implementation.

### Container

| M3 Property | M3 Value | Implementation |
|-------------|----------|----------------|
| Color | `inverseSurface` | `SnackbarBackground` -> `InverseSurfaceBrush` |
| Shape | Extra Small (4dp) | `SnackbarCornerRadius` = 4 |
| Elevation | Level 3 (6dp) | ThemeShadow on host presenter |
| Min width | 344dp | `SnackbarMinWidth` = 344 |
| Max width | 672dp | `SnackbarMaxWidth` = 672 |
| Mobile (compact) | Full width minus margins | Host margin 8dp left/right |

### Layout and Spacing

| M3 Property | M3 Value | Implementation |
|-------------|----------|----------------|
| Leading padding | 16dp | Container Padding left = 16 |
| Trailing padding (no action) | 16dp | `SnackbarPadding` = `16,14,16,14` |
| Trailing padding (with action/dismiss) | 8dp | `SnackbarPaddingWithActions` = `16,14,8,14` (via VisualState) |
| Top/bottom padding | 14dp | Container Padding top/bottom = 14 |
| Text-to-action spacing | 8dp | Message text right padding = 8 |
| Action-to-dismiss spacing | 0dp | Buttons adjacent in Grid columns |
| Bottom margin | 8dp from edge | `SnackbarHostMargin` = `8,0,8,8` |

### Supporting Text

| M3 Property | M3 Value | Implementation |
|-------------|----------|----------------|
| Color | `inverseOnSurface` | `SnackbarMessageForeground` -> `InverseOnSurfaceBrush` |
| Typography | Body Medium | `BodyMediumFontFamily/Weight/Size/CharacterSpacing` |
| Font size | 14sp | `SnackbarMessageFontSize` = 14 |
| Line height | 20sp | `LineHeight="20"` on TextBlock |
| Letter spacing | 0.25sp | `BodyMediumCharacterSpacing` token |
| Font weight | 400 (Regular) | `BodyMediumFontWeight` token |
| Max lines | 2 | `MaxLines="2"` on TextBlock |
| Text overflow | Ellipsis | `TextTrimming="CharacterEllipsis"` |

### Action Button

| M3 Property | M3 Value | Implementation |
|-------------|----------|----------------|
| Label color | `inversePrimary` | `SnackbarActionForeground` -> `InversePrimaryBrush` |
| Typography | Label Large | `LabelLargeFontFamily/Weight/Size` |
| Font size | 14sp | `SnackbarActionFontSize` = 14 |
| Font weight | 500 (Medium) | `FontWeight="Medium"` |
| Min touch target | 48dp | `MinHeight="48"` on Button |

### Dismiss Icon Button

| M3 Property | M3 Value | Implementation |
|-------------|----------|----------------|
| Icon | close (X) | FontIcon glyph `\uE711` |
| Icon color | `inverseOnSurface` | `SnackbarDismissForeground` -> `InverseOnSurfaceBrush` |
| Icon size | 24dp | `SnackbarDismissIconSize` = 24 |
| Button size | 48 x 48dp | `Width="48" Height="48"` |
| Min touch target | 48dp | Same as button size |

### Motion

| M3 Property | M3 Value | Implementation |
|-------------|----------|----------------|
| Enter duration | 150ms | `EnterAnimationDuration` = 150ms |
| Exit duration | 75ms | `ExitAnimationDuration` = 75ms |
| Enter easing | Emphasized Decelerate | `ExponentialEase { Exponent=4.5, EaseOut }` |
| Exit easing | Emphasized Accelerate | `ExponentialEase { Exponent=4.5, EaseIn }` |

### Behavior

| M3 Property | M3 Value | Implementation |
|-------------|----------|----------------|
| Default timeout | 4000-10000ms (typically 5000ms) | `SnackbarDuration.Short` = 4s |
| Indefinite | Supported | `SnackbarDuration.Indefinite` = `TimeSpan.MaxValue` |
| Max on screen | 1 | Queue-based, one visible at a time |
| Dismiss gesture | Swipe off-screen | `IsSwipeToDismissEnabled` (horizontal swipe) |
| Entrance animation | Slide up from bottom | TranslateY 80->0 + opacity fade |
| Exit animation | Slide down / fade out | TranslateY 0->80 + opacity fade |

### Color Token Mapping

| Element | Light Theme | Dark Theme |
|---------|------------|------------|
| Container | `inverseSurface` | `inverseSurface` |
| Supporting text | `inverseOnSurface` | `inverseOnSurface` |
| Action label | `inversePrimary` | `inversePrimary` |
| Dismiss icon | `inverseOnSurface` | `inverseOnSurface` |

## SnackbarHost

`SnackbarHost` is a `ContentControl` that manages the snackbar queue, animations, and positioning. Wrap your page content inside it.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultDuration` | `TimeSpan` | `SnackbarDuration.Short` (4s) | Default auto-dismiss duration when `SnackbarItem.Duration` is null |
| `SnackbarStyle` | `Style` | `null` | Style applied to each `Snackbar` control instance |
| `IsSwipeToDismissEnabled` | `bool` | `true` | Whether swiping horizontally dismisses the snackbar |
| `MaxQueueSize` | `int` | `5` | Maximum number of pending snackbars in the queue |
| `IsShowing` | `bool` | `false` | (Read-only) Whether a snackbar is currently visible |

### Attached Properties

| Property | Type | Description |
|----------|------|-------------|
| `SnackbarHost.Host` | `SnackbarHost` | Attach a `SnackbarHost` reference to any element for convenient access |

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `ShowAsync(SnackbarItem)` | `Task<SnackbarDismissReason>` | Shows a snackbar and returns how it was dismissed |
| `ShowAsync(string, string?, ICommand?, TimeSpan?)` | `Task<SnackbarDismissReason>` | Convenience overload for simple messages |
| `Dismiss()` | `void` | Programmatically dismisses the current snackbar |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `SnackbarOpened` | `EventHandler<SnackbarItem>` | Raised when a snackbar becomes visible |
| `SnackbarClosed` | `EventHandler<SnackbarItem>` | Raised when a snackbar is dismissed |

## Snackbar

`Snackbar` is the visual control rendered inside `SnackbarHost`. You typically do not create it directly; `SnackbarHost.ShowAsync` creates it from a `SnackbarItem`.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Message` | `string` | `""` | The message text displayed |
| `ActionLabel` | `string?` | `null` | Label for the action button; hidden when null or empty |
| `ActionCommand` | `ICommand?` | `null` | Command executed when the action button is clicked |
| `ActionCommandParameter` | `object?` | `null` | Parameter passed to `ActionCommand` |
| `ShowDismissButton` | `bool` | `false` | Whether to show a close (X) button |
| `IsActionOnNewLine` | `bool` | `false` | Whether the action button appears on a separate line below the message |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `ActionClicked` | `EventHandler` | Raised when the action button is clicked |
| `DismissClicked` | `EventHandler` | Raised when the dismiss button is clicked or Escape is pressed |

## SnackbarItem

A POCO that describes the content and behavior of a single snackbar notification.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Message` | `string` | `""` | The message text |
| `ActionLabel` | `string?` | `null` | Action button label |
| `ActionCommand` | `ICommand?` | `null` | Action command |
| `ActionCommandParameter` | `object?` | `null` | Action command parameter |
| `ShowDismissButton` | `bool` | `false` | Show dismiss button |
| `IsActionOnNewLine` | `bool` | `false` | Place action on new line |
| `Duration` | `TimeSpan?` | `null` | Display duration (null uses host default) |

## SnackbarDismissReason

| Value | Description |
|-------|-------------|
| `Timeout` | Auto-dismissed after duration elapsed |
| `Action` | User clicked the action button |
| `Dismiss` | User explicitly dismissed (close button, swipe, or Escape) |
| `Replaced` | Displaced by another snackbar in the queue |

## SnackbarDuration

| Constant | Value | Description |
|----------|-------|-------------|
| `Short` | 4 seconds | Default brief display (M3 minimum) |
| `Long` | 7 seconds | Extended display |
| `Indefinite` | `TimeSpan.MaxValue` | Stays until explicitly dismissed |

## Lightweight Styling

The following ThemeResource keys can be overridden to customize appearance:

### Container

| Key | Type | Default (M3) | Description |
|-----|------|-------------|-------------|
| `SnackbarBackground` | `Brush` | `inverseSurface` | Background color |
| `SnackbarBorderBrush` | `Brush` | Transparent | Border color |
| `SnackbarCornerRadius` | `CornerRadius` | 4 (Extra Small) | Corner rounding |
| `SnackbarPadding` | `Thickness` | `16,14,16,14` | Padding when no action/dismiss |
| `SnackbarPaddingWithActions` | `Thickness` | `16,14,8,14` | Padding when action or dismiss visible |
| `SnackbarMinHeight` | `Double` | 48 | Minimum height |
| `SnackbarMinWidth` | `Double` | 344 | Minimum width |
| `SnackbarMaxWidth` | `Double` | 672 | Maximum width |

### Message

| Key | Type | Default (M3) | Description |
|-----|------|-------------|-------------|
| `SnackbarMessageForeground` | `Brush` | `inverseOnSurface` | Message text color |
| `SnackbarMessageFontSize` | `Double` | 14 (Body Medium) | Message font size |

### Action Button

| Key | Type | Default (M3) | Description |
|-----|------|-------------|-------------|
| `SnackbarActionForeground` | `Brush` | `inversePrimary` | Action button text color |
| `SnackbarActionFontSize` | `Double` | 14 (Label Large) | Action button font size |

### Dismiss Button

| Key | Type | Default (M3) | Description |
|-----|------|-------------|-------------|
| `SnackbarDismissForeground` | `Brush` | `inverseOnSurface` | Dismiss icon color |
| `SnackbarDismissIconSize` | `Double` | 24 | Dismiss icon size |

### Host

| Key | Type | Default (M3) | Description |
|-----|------|-------------|-------------|
| `SnackbarHostMargin` | `Thickness` | `8,0,8,8` | Margin around the snackbar within the host |

## Material Styles

| Style Key | Target |
|-----------|--------|
| `MaterialSnackbarStyle` | `Snackbar` |
| `MaterialSnackbarHostStyle` | `SnackbarHost` |

The Material styles use M3 inverse token aliasing (`InverseSurfaceBrush`, `InverseOnSurfaceBrush`, `InversePrimaryBrush`) with full Default/Light/Dark ThemeDictionary support.

## Cupertino Styles

| Style Key | Target |
|-----------|--------|
| `CupertinoSnackbarStyle` | `Snackbar` |
| `CupertinoSnackbarHostStyle` | `SnackbarHost` |

The Cupertino styles use iOS conventions: 12dp corner radius, semi-transparent dark pill background, iOS system blue action color (#0A84FF), and 15px font size.
