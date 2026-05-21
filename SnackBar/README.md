# Snackbar Component for Uno Toolkit

A proposed `Snackbar` and `SnackbarHost` control for the [Uno Platform](https://platform.uno) Toolkit, following the [Material Design 3 Snackbar specification](https://m3.material.io/components/snackbar/overview). This repo contains the full component source, theme styles, a runnable test app, runtime tests, and documentation - everything needed for review before integration into [Uno.Toolkit.UI](https://github.com/nicasioca/uno.toolkit.ui).

## Repository Structure

```
SnackBar/
├── src/
│   ├── Uno.Toolkit.UI/Controls/Snackbar/   # Component source (drop into toolkit)
│   │   ├── Snackbar.cs                      # Visual control (TemplateParts, VisualStates)
│   │   ├── Snackbar.Properties.cs           # Dependency properties
│   │   ├── Snackbar.xaml                    # Default/base control template
│   │   ├── SnackbarHost.cs                  # Queue, animations, swipe-to-dismiss
│   │   ├── SnackbarHost.Properties.cs       # Host dependency & attached properties
│   │   ├── SnackbarHost.xaml                # Host template (content + overlay)
│   │   ├── SnackbarItem.cs                  # POCO describing a notification
│   │   ├── SnackbarDismissReason.cs         # Enum: Timeout, Action, Dismiss, Replaced
│   │   └── SnackbarDuration.cs              # Constants: Short (4s), Long (7s), Indefinite
│   └── Uno.Toolkit.RuntimeTests/Tests/
│       └── SnackbarTests.cs                 # Unit tests (MSTest, RunsOnUIThread)
│
├── src/library/
│   ├── Uno.Toolkit.Material/Styles/Controls/v2/
│   │   └── Snackbar.xaml                    # Material theme (M3 inverse tokens)
│   └── Uno.Toolkit.Cupertino/Styles/Controls/
│       └── Snackbar.xaml                    # Cupertino theme (iOS conventions)
│
├── samples/
│   └── Uno.Toolkit.Samples/.../Content/Controls/
│       ├── SnackbarSamplePage.xaml          # Sample page for the toolkit sample app
│       └── SnackbarSamplePage.xaml.cs
│
├── testapp/                                 # Standalone runnable test app (see below)
│   ├── SnackbarTestApp.sln
│   └── SnackbarTestApp/
│
├── doc/controls/
│   └── Snackbar.md                          # Full API documentation
│
├── snackbar-component-plan.md               # Original design/implementation plan
└── snackbar-mockup.html                     # Visual mockup
```

## Quick Start - Running the Test App

The `testapp/` directory contains a standalone Uno Platform app that embeds the Snackbar source files so you can build and run without cloning the full toolkit.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Uno Platform templates: `dotnet new install Uno.Templates`
- Run `uno-check` to verify your environment

### Build and Run

```bash
cd testapp/SnackbarTestApp
dotnet build -f net10.0-desktop
dotnet run -f net10.0-desktop
```

The app shows 7 buttons that exercise every snackbar variant:

| Button | What it tests |
|--------|---------------|
| **Basic Message** | Simple text, auto-dismiss (4s) |
| **With Action** | Message + "Undo" action button |
| **Action + Dismiss Button** | Action button + close (X) button |
| **Multi-line (Action on New Line)** | Long text with action below the message |
| **Long Duration (7s)** | `SnackbarDuration.Long` |
| **Indefinite (Must Dismiss)** | Stays until user taps dismiss or Escape |
| **Queue 3 Messages** | Three snackbars queued; displayed sequentially |

The dismiss reason (`Timeout`, `Action`, `Dismiss`, `Replaced`) is shown below the buttons after each interaction.

### Test App Notes

- **DispatcherCompat shim**: The test app includes `Helpers/DispatcherCompat.cs`, a lightweight wrapper around `DispatcherQueue`. This is needed because the real `DispatcherCompat` helper in Uno.Toolkit.UI is `internal`. When integrating into the toolkit proper, delete this shim - the source files will use the existing internal helper.
- **IS_WINUI define**: Added to the `.csproj` because the source files use `#if IS_WINUI` for WinUI vs UWP namespace selection. The Uno SDK does not define this automatically.
- **Material styles**: The test app uses hardcoded M3 color values (not `StaticResource` aliases like `InverseSurfaceBrush`) to avoid resource resolution ordering issues at startup. The actual `src/library/` Material styles use proper token aliasing and should work correctly when loaded within the full toolkit resource chain.

## Component Design

### Architecture

```
SnackbarHost (ContentControl)
├── Your page content (via Content property)
└── PART_SnackbarPresenter (overlay, bottom-aligned)
    └── Snackbar (Control) - created per ShowAsync() call
        ├── SingleLineRoot: [Message] [Action] [Dismiss]
        └── MultiLineRoot:  [Message ........... Dismiss]
                            [              Action]
```

### Key Behaviors

- **Async API**: `ShowAsync()` returns `Task<SnackbarDismissReason>` so callers can react to how the snackbar was dismissed
- **Queue**: Multiple `ShowAsync()` calls are queued and displayed sequentially. `MaxQueueSize` (default 5) drops oldest items when exceeded
- **Animations**: Slide-up enter (250ms) and slide-down exit (200ms) with opacity fade, using `CubicEase`
- **Swipe-to-dismiss**: Horizontal swipe with opacity feedback; threshold is 1/3 of presenter width
- **Keyboard**: Escape key dismisses the current snackbar
- **Accessibility**: `AutomationPeer` exposes the control as `StatusBar`; message text uses `AutomationProperties.LiveSetting="Assertive"`

### Visual States

**Snackbar control:**
- `ActionStates`: `NoAction` | `WithAction` | `WithActionOnNewLine`
- `DismissStates`: `NoDismiss` | `WithDismiss`

**SnackbarHost:**
- `DisplayStates`: `Hidden` | `Visible`

## Theme Styles

### Material (M3)

Uses inverse surface tokens for the dark pill appearance. Full Light/Dark theme dictionary support.

| Style Key | Target Type |
|-----------|-------------|
| `MaterialSnackbarStyle` | `Snackbar` |
| `MaterialSnackbarHostStyle` | `SnackbarHost` |

### Cupertino (iOS)

Uses iOS conventions: 12dp corner radius, semi-transparent dark background, system blue (#0A84FF) action color, 15px font size.

| Style Key | Target Type |
|-----------|-------------|
| `CupertinoSnackbarStyle` | `Snackbar` |
| `CupertinoSnackbarHostStyle` | `SnackbarHost` |

### Lightweight Styling

All visual properties are overridable via `ThemeResource` keys. See [doc/controls/Snackbar.md](doc/controls/Snackbar.md) for the full list of keys including `SnackbarBackground`, `SnackbarMessageForeground`, `SnackbarActionForeground`, `SnackbarCornerRadius`, etc.

## Runtime Tests

`src/Uno.Toolkit.RuntimeTests/Tests/SnackbarTests.cs` contains 8 MSTest tests covering:

- Default property values for `Snackbar`, `SnackbarHost`, and `SnackbarItem`
- `SnackbarDuration` constants
- `ShowAsync` returning `Timeout` after duration elapses
- `Dismiss()` returning `SnackbarDismissReason.Dismiss`
- Queue processing (3 sequential snackbars)
- `MaxQueueSize` overflow behavior

These tests use `[RunsOnUIThread]` and require the toolkit's `UIHelper.Load()` test infrastructure.

## Review Checklist

For reviewers evaluating this component for toolkit integration:

- [ ] **API surface**: Do the public types (`SnackbarHost`, `Snackbar`, `SnackbarItem`, `SnackbarDismissReason`, `SnackbarDuration`) follow toolkit naming conventions?
- [ ] **Namespace**: All types are in `Uno.Toolkit.UI` - confirm no conflicts with existing toolkit types
- [ ] **DispatcherCompat usage**: `SnackbarHost.cs` line 66 calls `this.GetDispatcherCompat()` - verify this works with the toolkit's internal helper
- [ ] **Template parts**: `PART_ActionButton`, `PART_DismissButton`, `PART_ActionButtonMultiLine`, `PART_DismissButtonMultiLine`, `PART_SnackbarPresenter` - naming consistent?
- [ ] **Material token aliasing**: The `src/library/` Material XAML uses `StaticResource ResourceKey="InverseSurfaceBrush"` etc. - confirm these M3 tokens exist in the current Material resource chain
- [ ] **Cupertino style**: Verify iOS color values and corner radius match current Cupertino guidelines
- [ ] **Animation timing**: Enter 250ms / Exit 200ms with CubicEase - acceptable for all platforms?
- [ ] **Accessibility**: `StatusBar` automation type, `LiveSetting="Assertive"` on message text
- [ ] **Sample page**: `SnackbarSamplePage` uses `SamplePageLayout` / `SampleCategory.Controls` - matches current sample app patterns?
- [ ] **Test coverage**: 8 runtime tests sufficient for initial merge?
- [ ] **Documentation**: `doc/controls/Snackbar.md` complete and accurate?

## Known Limitations

- `AutomationProperties.SetLiveSetting` is not yet implemented in Uno Platform (generates a build warning but does not affect functionality)
- Swipe-to-dismiss uses `ManipulationMode.TranslateX` which may behave differently on touch vs mouse input
- The `RegisterDragDrop` warning on Win32 desktop is a known Uno Platform Skia host issue and is harmless
