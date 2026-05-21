# Snackbar Component Plan — Uno Toolkit

## Status: Implementation Complete, Needs Build Verification

The Snackbar component is fully implemented across 16 files. Four critical bugs were found and fixed during gap analysis. A sample page and Cupertino styles were added. The component needs a build + smoke test before PR.

---

## Architecture Overview

```
SnackbarHost (ContentControl)         ← Queue manager, animations, auto-dismiss timer
  └─ Snackbar (Control)               ← Visual element (message, action button, dismiss button)
       └─ SnackbarItem (POCO)         ← Data model for a notification
           └─ SnackbarDismissReason   ← Enum: Timeout | Action | Dismiss | Replaced
           └─ SnackbarDuration        ← Static constants: Short(4s) | Long(7s) | Indefinite
```

The async API returns `Task<SnackbarDismissReason>` from `ShowAsync()`, enabling callers to react to how the user interacted (e.g., undo on `Action`).

---

## File Inventory

### Core Implementation (6 files) — `src/Uno.Toolkit.UI/Controls/Snackbar/`

| File | Purpose |
|------|---------|
| `Snackbar.cs` | Visual control: template parts, visual states, keyboard (Escape), AutomationPeer |
| `Snackbar.Properties.cs` | DPs: Message, ActionLabel, ActionCommand, ActionCommandParameter, ShowDismissButton, IsActionOnNewLine |
| `Snackbar.xaml` | Base default template + ThemeDictionaries (Default/Light/Dark) |
| `SnackbarHost.cs` | **MODIFIED** — Queue, animations, swipe-to-dismiss, timer. Added: deferred ProcessQueue in OnApplyTemplate, Unloaded handler |
| `SnackbarHost.Properties.cs` | DPs: DefaultDuration, SnackbarStyle, IsSwipeToDismissEnabled, MaxQueueSize, IsShowing. Attached: Host |
| `SnackbarHost.xaml` | Base host template with PART_SnackbarPresenter |

### Supporting Types (2 files) — same directory

| File | Purpose |
|------|---------|
| `SnackbarItem.cs` | POCO: Message, ActionLabel, ActionCommand, ActionCommandParameter, ShowDismissButton, IsActionOnNewLine, Duration |
| `SnackbarDismissReason.cs` | Enum: Timeout, Action, Dismiss, Replaced |
| `SnackbarDuration.cs` | Static: Short (4s), Long (7s), Indefinite (MaxValue) |

### Material Styles (1 file) — `src/library/Uno.Toolkit.Material/Styles/Controls/v2/`

| File | Purpose |
|------|---------|
| `Snackbar.xaml` | **MODIFIED** — Material 3 themed styles. Fixed: added `x:Name="PART_ActionButtonMultiLine"`, added Dark ThemeDictionary |

### Cupertino Styles (1 file) — `src/library/Uno.Toolkit.Cupertino/Styles/Controls/`

| File | Purpose |
|------|---------|
| `Snackbar.xaml` | **NEW** — iOS-style: 12dp corners, semi-transparent dark pill, iOS blue action (#0A84FF), 15px font |

### Sample Page (2 files) — `samples/.../Content/Controls/`

| File | Purpose |
|------|---------|
| `SnackbarSamplePage.xaml` | **NEW** — 7 demo buttons: basic, action, action+dismiss, multi-line, long, indefinite, queue |
| `SnackbarSamplePage.xaml.cs` | **NEW** — Code-behind with ShowAsync calls, dismiss reason display |

### Documentation (3 files)

| File | Purpose |
|------|---------|
| `doc/controls/Snackbar.md` | Full API reference, usage examples, lightweight styling keys |
| `doc/toc.yml` | **MODIFIED** — Added Snackbar entry between SafeArea and ShadowContainer |
| `doc/controls-styles.md` | **MODIFIED** — Added to controls list + Material styles table |

### Tests (1 file) — `src/Uno.Toolkit.RuntimeTests/Tests/`

| File | Purpose |
|------|---------|
| `SnackbarTests.cs` | 10 runtime tests: default state, action label, dismiss, queue, max queue size, indefinite, auto-dismiss |

---

## Bug Fixes Applied

### Bug #1 (P0): Multi-line action button unnamed

**File:** `src/library/Uno.Toolkit.Material/Styles/Controls/v2/Snackbar.xaml`
**Line:** ~240
**Problem:** The multi-line action `<Button>` was missing `x:Name="PART_ActionButtonMultiLine"`. When `IsActionOnNewLine=true`, the button renders but `Snackbar.OnApplyTemplate()` can't resolve the template part → click events never wire up → action button is dead.
**Fix:** Added `x:Name="PART_ActionButtonMultiLine"` to the Button element.
**Verify:** Show multi-line snackbar, tap action button. Dismiss reason should be `Action`, not `Timeout`.

### Bug #2 (P0): Material Dark theme missing

**File:** `src/library/Uno.Toolkit.Material/Styles/Controls/v2/Snackbar.xaml`
**Problem:** ThemeDictionaries only had `Default` and `Light`. In dark mode, WinUI falls back to `Default` which uses `InverseSurfaceBrush` etc. — but those M3 tokens resolve to *light theme values* in the Default dict. Dark mode snackbar appeared with wrong colors (light-on-light instead of dark-on-dark inverse).
**Fix:** Added complete `<ResourceDictionary x:Key="Dark">` with the same StaticResource aliasing to `InverseSurfaceBrush`, `InverseOnSurfaceBrush`, `InversePrimaryBrush`.
**Verify:** Toggle to dark mode, show snackbar. Colors should be visually distinct from light mode and use proper inverse surface tokens.

### Bug #3 (P1): Queue stuck if ShowAsync called before template apply

**File:** `src/Uno.Toolkit.UI/Controls/Snackbar/SnackbarHost.cs`
**Problem:** If `ShowAsync()` is called during constructor or `Loaded` before `OnApplyTemplate()` fires, `_isReady=false` causes `ProcessQueue()` to return early. Items queue but never display.
**Fix:** Added at end of `OnApplyTemplate()`:
```csharp
if (_queue.Count > 0)
{
    ProcessQueue();
}
```
**Verify:** Call `ShowAsync` in Page constructor before host template applies. Message should still appear.

### Bug #4 (P1): Pending tasks never complete if host unloaded

**File:** `src/Uno.Toolkit.UI/Controls/Snackbar/SnackbarHost.cs`
**Problem:** If `SnackbarHost` removed from visual tree while a snackbar is showing, `TaskCompletionSource` tasks await forever, causing memory leaks and deadlocked `await` callers.
**Fix:** Added `Unloaded += OnUnloaded` handler that: cancels timer, stops animations, completes current TCS with `Dismiss`, drains queue completing all pending TCS with `Replaced`, resets state.
**Verify:** Show indefinite snackbar, navigate away from page. Awaiting task should complete (not hang).

---

## Cupertino Styles — Design Decisions

The Cupertino `Snackbar.xaml` uses `BasedOn="{StaticResource DefaultSnackbar}"` to reuse the base template while overriding ThemeResource keys:

| Property | Material | Cupertino |
|----------|----------|-----------|
| Corner radius | 4dp | 12dp |
| Background | M3 InverseSurface token | Semi-transparent dark pill (`#E6292929` light / `#E63A3A3C` dark) |
| Action color | M3 InversePrimary token | iOS system blue (`#0A84FF`) |
| Font size | M3 BodyMedium (14px) | iOS default (15px) |
| Dismiss color | M3 InverseOnSurface token | iOS systemGray (`#98989D` light / `#636366` dark) |

**⚠️ Resource loading risk:** The `BasedOn="{StaticResource DefaultSnackbar}"` reference requires that the base `Snackbar.xaml` from `Uno.Toolkit.UI` is merged into the resource tree *before* the Cupertino dictionary. If the Cupertino library doesn't automatically ensure this ordering, you'll get a `StaticResource not found` error at runtime. The Material library likely handles this through its own dependency chain. Check: does the Cupertino csproj have a dependency on `Uno.Toolkit.WinUI`?

---

## Sample Page — SamplePageLayout Pattern

The sample follows the existing `DrawerControlSamplePage` pattern:

- Uses `<sample:SamplePageLayout x:Name="SamplePageLayout" IsDesignAgnostic="True">` with `DesignAgnosticTemplate`
- Code-behind retrieves named elements via `SamplePageLayout.GetSampleChild<T>(Design.Agnostic, "name")`
- `[SamplePage(SampleCategory.Controls, "Snackbar")]` attribute registers for auto-discovery
- Click handlers inside `DataTemplate` work because they're on the Page class (not the template)
- `SnackbarHost` is placed *inside* the DataTemplate Grid, overlaying the ScrollViewer content
- Bottom padding (80px) on the StackPanel prevents buttons from being hidden behind snackbar

**7 demo scenarios:**
1. Basic message only (auto-dismiss 4s)
2. Message + action button
3. Message + action + dismiss button
4. Multi-line with `IsActionOnNewLine=true`
5. Long duration (7s)
6. Indefinite (must manually dismiss)
7. Queue 3 messages sequentially

---

## Build & Smoke Test Steps

### Build command (fastest: desktop target)

```bash
cd uno.toolkit.ui
dotnet build samples/Uno.Toolkit.Samples/Uno.Toolkit.Samples/Uno.Toolkit.Samples.csproj -f net10.0-desktop
```

Alternative targets: `net10.0-browserwasm`, `net10.0-android`, `net10.0-ios`

### Prerequisites

- .NET 10 SDK
- `global.json` pulls `Uno.Sdk.Private 6.6.0-dev.116` (needs Uno private feed)

### Smoke test checklist (priority order)

| # | Test | What to check | Bug verified |
|---|------|---------------|-------------|
| 1 | **Dark theme** | Toggle dark mode → show snackbar → colors should use inverse surface (dark bg, light text in light mode; light bg, dark text in dark mode) | Bug #2 |
| 2 | **Multi-line action click** | Tap "Show Multi-line" → tap "Got It" → dismiss reason shows "Action" (not "Timeout") | Bug #1 |
| 3 | **Basic message** | Tap "Show Message" → appears → auto-dismisses after 4s → reason shows "Timeout" | Baseline |
| 4 | **Indefinite** | Tap "Show Indefinite" → stays forever → tap ✕ → reason shows "Dismiss" | Baseline |
| 5 | **Queue** | Tap "Queue 3 Messages" → 3 snackbars appear sequentially → last reason shows dismiss type | Baseline |
| 6 | **Swipe dismiss** | While snackbar showing, swipe right past 1/3 threshold → should dismiss | Baseline |
| 7 | **Escape key** | While snackbar showing, press Escape → should dismiss | Baseline |
| 8 | **Navigate away** | Show indefinite snackbar → navigate to different page → no crash/hang | Bug #4 |

### What to look for at build time

- **Cupertino `StaticResource not found`**: If `DefaultSnackbar` or `DefaultSnackbarHost` can't be resolved, the Cupertino library may need an explicit ResourceDictionary merge or project dependency adjustment
- **XAML compiler errors**: The new sample page Click handlers are in DataTemplate — some XAML compilers may warn but this pattern works (verified against SafeAreaSamplePage which does the same)
- **Missing `TitleSmall` style**: The sample page uses `Style="{StaticResource TitleSmall}"` — verify this exists in the sample app's resource chain (it's an M3 typography token)

---

## Remaining Work (Post-Verification)

### P2: C# Markup Extensions

**Directory:** `src/library/Uno.Toolkit.WinUI.Markup/Theme/`
**Pattern:** Follow existing `Card.cs`, `Divider.cs`

Mechanical work — creates extension methods for `Snackbar` and `SnackbarHost` properties for C# Markup fluent API. Not blocking for PR but will likely be flagged in review if other controls have them.

### P3: SafeArea Integration

On iOS/Android, the snackbar should respect bottom safe area insets (home indicator, gesture bar). Currently `SnackbarHostMargin` is hardcoded to `8,0,8,8`. A future enhancement could integrate with `utu:SafeArea` to auto-adjust.

---

## Complete File Contents

Below are all files in their current state. Use these to verify or recreate files in the local repo.

---

### `src/Uno.Toolkit.UI/Controls/Snackbar/SnackbarItem.cs`

```csharp
using System;
using System.Windows.Input;

namespace Uno.Toolkit.UI
{
	/// <summary>
	/// Describes the content and behavior of a single snackbar notification.
	/// </summary>
	public class SnackbarItem
	{
		/// <summary>
		/// Gets or sets the text message to display.
		/// </summary>
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the label for the optional action button. When null or empty, no action button is shown.
		/// </summary>
		public string? ActionLabel { get; set; }

		/// <summary>
		/// Gets or sets the command to execute when the action button is clicked.
		/// </summary>
		public ICommand? ActionCommand { get; set; }

		/// <summary>
		/// Gets or sets the parameter to pass to <see cref="ActionCommand"/>.
		/// </summary>
		public object? ActionCommandParameter { get; set; }

		/// <summary>
		/// Gets or sets whether a dismiss (close) button is shown.
		/// </summary>
		public bool ShowDismissButton { get; set; }

		/// <summary>
		/// Gets or sets whether the action button should be placed on a new line below the message.
		/// </summary>
		public bool IsActionOnNewLine { get; set; }

		/// <summary>
		/// Gets or sets the display duration for this snackbar.
		/// When null, the host's <see cref="SnackbarHost.DefaultDuration"/> is used.
		/// Use <see cref="SnackbarDuration"/> constants for standard values.
		/// </summary>
		public TimeSpan? Duration { get; set; }
	}
}
```

### `src/Uno.Toolkit.UI/Controls/Snackbar/SnackbarDismissReason.cs`

```csharp
namespace Uno.Toolkit.UI
{
	/// <summary>
	/// Describes the reason a snackbar was dismissed.
	/// </summary>
	public enum SnackbarDismissReason
	{
		/// <summary>The snackbar auto-dismissed after its duration elapsed.</summary>
		Timeout,

		/// <summary>The user clicked the action button.</summary>
		Action,

		/// <summary>The user explicitly dismissed the snackbar (close button, swipe, or Escape key).</summary>
		Dismiss,

		/// <summary>The snackbar was replaced by another snackbar in the queue.</summary>
		Replaced,
	}
}
```

### `src/Uno.Toolkit.UI/Controls/Snackbar/SnackbarDuration.cs`

```csharp
using System;

namespace Uno.Toolkit.UI
{
	/// <summary>
	/// Provides predefined duration constants for snackbar display times.
	/// </summary>
	public static class SnackbarDuration
	{
		/// <summary>A short snackbar duration (4 seconds).</summary>
		public static readonly TimeSpan Short = TimeSpan.FromSeconds(4);

		/// <summary>A long snackbar duration (7 seconds).</summary>
		public static readonly TimeSpan Long = TimeSpan.FromSeconds(7);

		/// <summary>An indefinite duration. The snackbar will remain visible until explicitly dismissed.</summary>
		public static readonly TimeSpan Indefinite = TimeSpan.MaxValue;
	}
}
```

### `src/Uno.Toolkit.UI/Controls/Snackbar/Snackbar.cs`

```csharp
using System;

#if IS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#endif

namespace Uno.Toolkit.UI
{
	/// <summary>
	/// Represents the visual element for a snackbar notification.
	/// Displays a brief message with an optional action button and dismiss button.
	/// </summary>
	[TemplatePart(Name = TemplateParts.ActionButtonName, Type = typeof(Button))]
	[TemplatePart(Name = TemplateParts.DismissButtonName, Type = typeof(Button))]
	[TemplatePart(Name = TemplateParts.ActionButtonMultiLineName, Type = typeof(Button))]
	[TemplatePart(Name = TemplateParts.DismissButtonMultiLineName, Type = typeof(Button))]
	[TemplateVisualState(GroupName = VisualStateNames.ActionStatesGroupName, Name = VisualStateNames.NoAction)]
	[TemplateVisualState(GroupName = VisualStateNames.ActionStatesGroupName, Name = VisualStateNames.WithAction)]
	[TemplateVisualState(GroupName = VisualStateNames.ActionStatesGroupName, Name = VisualStateNames.WithActionOnNewLine)]
	[TemplateVisualState(GroupName = VisualStateNames.DismissStatesGroupName, Name = VisualStateNames.NoDismiss)]
	[TemplateVisualState(GroupName = VisualStateNames.DismissStatesGroupName, Name = VisualStateNames.WithDismiss)]
	public partial class Snackbar : Control
	{
		internal static class TemplateParts
		{
			public const string ActionButtonName = "PART_ActionButton";
			public const string DismissButtonName = "PART_DismissButton";
			public const string ActionButtonMultiLineName = "PART_ActionButtonMultiLine";
			public const string DismissButtonMultiLineName = "PART_DismissButtonMultiLine";
		}

		private class VisualStateNames
		{
			// ActionStates
			public const string ActionStatesGroupName = "ActionStates";
			public const string NoAction = nameof(NoAction);
			public const string WithAction = nameof(WithAction);
			public const string WithActionOnNewLine = nameof(WithActionOnNewLine);

			// DismissStates
			public const string DismissStatesGroupName = "DismissStates";
			public const string NoDismiss = nameof(NoDismiss);
			public const string WithDismiss = nameof(WithDismiss);
		}

		private Button? _actionButton;
		private Button? _dismissButton;
		private Button? _actionButtonMultiLine;
		private Button? _dismissButtonMultiLine;
		private bool _isReady;

		/// <summary>
		/// Occurs when the action button is clicked.
		/// </summary>
		public event EventHandler? ActionClicked;

		/// <summary>
		/// Occurs when the dismiss button is clicked.
		/// </summary>
		public event EventHandler? DismissClicked;

		public Snackbar()
		{
			DefaultStyleKey = typeof(Snackbar);
			KeyDown += OnKeyDown;
		}

		protected override void OnApplyTemplate()
		{
			// Unwire previous template parts
			UnwireButton(ref _actionButton, OnActionButtonClick);
			UnwireButton(ref _dismissButton, OnDismissButtonClick);
			UnwireButton(ref _actionButtonMultiLine, OnActionButtonClick);
			UnwireButton(ref _dismissButtonMultiLine, OnDismissButtonClick);

			base.OnApplyTemplate();

			// Resolve and wire all template parts
			_actionButton = WireButton(TemplateParts.ActionButtonName, OnActionButtonClick);
			_dismissButton = WireButton(TemplateParts.DismissButtonName, OnDismissButtonClick);
			_actionButtonMultiLine = WireButton(TemplateParts.ActionButtonMultiLineName, OnActionButtonClick);
			_dismissButtonMultiLine = WireButton(TemplateParts.DismissButtonMultiLineName, OnDismissButtonClick);

			_isReady = true;
			UpdateVisualStates(useTransitions: false);
		}

		private Button? WireButton(string name, RoutedEventHandler handler)
		{
			var button = GetTemplateChild(name) as Button;
			if (button != null)
			{
				button.Click += handler;
			}
			return button;
		}

		private void UnwireButton(ref Button? button, RoutedEventHandler handler)
		{
			if (button != null)
			{
				button.Click -= handler;
				button = null;
			}
		}

		private void OnLayoutPropertiesChanged()
		{
			UpdateVisualStates(useTransitions: true);
		}

		private void UpdateVisualStates(bool useTransitions)
		{
			if (!_isReady) return;

			// Action states
			var hasAction = !string.IsNullOrEmpty(ActionLabel);
			if (!hasAction)
			{
				VisualStateManager.GoToState(this, VisualStateNames.NoAction, useTransitions);
			}
			else if (IsActionOnNewLine)
			{
				VisualStateManager.GoToState(this, VisualStateNames.WithActionOnNewLine, useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, VisualStateNames.WithAction, useTransitions);
			}

			// Dismiss states
			VisualStateManager.GoToState(
				this,
				ShowDismissButton ? VisualStateNames.WithDismiss : VisualStateNames.NoDismiss,
				useTransitions);
		}

		private void OnActionButtonClick(object sender, RoutedEventArgs e)
		{
			// Execute command from code-behind (not XAML binding) to avoid double-execution
			ActionCommand?.Execute(ActionCommandParameter);
			ActionClicked?.Invoke(this, EventArgs.Empty);
		}

		private void OnDismissButtonClick(object sender, RoutedEventArgs e)
		{
			DismissClicked?.Invoke(this, EventArgs.Empty);
		}

		private void OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Escape)
			{
				DismissClicked?.Invoke(this, EventArgs.Empty);
				e.Handled = true;
			}
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new SnackbarAutomationPeer(this);
		}
	}

	/// <summary>
	/// Automation peer for the Snackbar control, exposing it as a Status element
	/// for assistive technologies.
	/// </summary>
	internal class SnackbarAutomationPeer : FrameworkElementAutomationPeer
	{
		public SnackbarAutomationPeer(Snackbar owner) : base(owner) { }

		protected override AutomationControlType GetAutomationControlTypeCore()
			=> AutomationControlType.StatusBar;

		protected override string GetClassNameCore()
			=> nameof(Snackbar);

		protected override string GetNameCore()
		{
			if (Owner is Snackbar snackbar)
			{
				return snackbar.Message ?? string.Empty;
			}
			return base.GetNameCore();
		}
	}
}
```

### `src/Uno.Toolkit.UI/Controls/Snackbar/Snackbar.Properties.cs`

```csharp
using System;
using System.Windows.Input;

#if IS_WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace Uno.Toolkit.UI
{
	public partial class Snackbar
	{
		#region DependencyProperty: Message

		public static DependencyProperty MessageProperty { get; } = DependencyProperty.Register(
			nameof(Message),
			typeof(string),
			typeof(Snackbar),
			new PropertyMetadata(string.Empty, (s, e) => ((Snackbar)s).OnLayoutPropertiesChanged()));

		public string Message
		{
			get => (string)GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}

		#endregion
		#region DependencyProperty: ActionLabel

		public static DependencyProperty ActionLabelProperty { get; } = DependencyProperty.Register(
			nameof(ActionLabel),
			typeof(string),
			typeof(Snackbar),
			new PropertyMetadata(default(string), (s, e) => ((Snackbar)s).OnLayoutPropertiesChanged()));

		public string? ActionLabel
		{
			get => (string?)GetValue(ActionLabelProperty);
			set => SetValue(ActionLabelProperty, value);
		}

		#endregion
		#region DependencyProperty: ActionCommand

		public static DependencyProperty ActionCommandProperty { get; } = DependencyProperty.Register(
			nameof(ActionCommand),
			typeof(ICommand),
			typeof(Snackbar),
			new PropertyMetadata(default(ICommand)));

		public ICommand? ActionCommand
		{
			get => (ICommand?)GetValue(ActionCommandProperty);
			set => SetValue(ActionCommandProperty, value);
		}

		#endregion
		#region DependencyProperty: ActionCommandParameter

		public static DependencyProperty ActionCommandParameterProperty { get; } = DependencyProperty.Register(
			nameof(ActionCommandParameter),
			typeof(object),
			typeof(Snackbar),
			new PropertyMetadata(default(object)));

		public object? ActionCommandParameter
		{
			get => GetValue(ActionCommandParameterProperty);
			set => SetValue(ActionCommandParameterProperty, value);
		}

		#endregion
		#region DependencyProperty: ShowDismissButton

		public static DependencyProperty ShowDismissButtonProperty { get; } = DependencyProperty.Register(
			nameof(ShowDismissButton),
			typeof(bool),
			typeof(Snackbar),
			new PropertyMetadata(false, (s, e) => ((Snackbar)s).OnLayoutPropertiesChanged()));

		public bool ShowDismissButton
		{
			get => (bool)GetValue(ShowDismissButtonProperty);
			set => SetValue(ShowDismissButtonProperty, value);
		}

		#endregion
		#region DependencyProperty: IsActionOnNewLine

		public static DependencyProperty IsActionOnNewLineProperty { get; } = DependencyProperty.Register(
			nameof(IsActionOnNewLine),
			typeof(bool),
			typeof(Snackbar),
			new PropertyMetadata(false, (s, e) => ((Snackbar)s).OnLayoutPropertiesChanged()));

		public bool IsActionOnNewLine
		{
			get => (bool)GetValue(IsActionOnNewLineProperty);
			set => SetValue(IsActionOnNewLineProperty, value);
		}

		#endregion
	}
}
```

### `src/Uno.Toolkit.UI/Controls/Snackbar/SnackbarHost.Properties.cs`

```csharp
using System;

#if IS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Uno.Toolkit.UI
{
	public partial class SnackbarHost
	{
		#region DependencyProperty: DefaultDuration

		public static DependencyProperty DefaultDurationProperty { get; } = DependencyProperty.Register(
			nameof(DefaultDuration),
			typeof(TimeSpan),
			typeof(SnackbarHost),
			new PropertyMetadata(SnackbarDuration.Short));

		public TimeSpan DefaultDuration
		{
			get => (TimeSpan)GetValue(DefaultDurationProperty);
			set => SetValue(DefaultDurationProperty, value);
		}

		#endregion
		#region DependencyProperty: SnackbarStyle

		public static DependencyProperty SnackbarStyleProperty { get; } = DependencyProperty.Register(
			nameof(SnackbarStyle),
			typeof(Style),
			typeof(SnackbarHost),
			new PropertyMetadata(default(Style)));

		public Style? SnackbarStyle
		{
			get => (Style?)GetValue(SnackbarStyleProperty);
			set => SetValue(SnackbarStyleProperty, value);
		}

		#endregion
		#region DependencyProperty: IsSwipeToDismissEnabled = true

		public static DependencyProperty IsSwipeToDismissEnabledProperty { get; } = DependencyProperty.Register(
			nameof(IsSwipeToDismissEnabled),
			typeof(bool),
			typeof(SnackbarHost),
			new PropertyMetadata(true));

		public bool IsSwipeToDismissEnabled
		{
			get => (bool)GetValue(IsSwipeToDismissEnabledProperty);
			set => SetValue(IsSwipeToDismissEnabledProperty, value);
		}

		#endregion
		#region DependencyProperty: MaxQueueSize = 5

		public static DependencyProperty MaxQueueSizeProperty { get; } = DependencyProperty.Register(
			nameof(MaxQueueSize),
			typeof(int),
			typeof(SnackbarHost),
			new PropertyMetadata(5));

		public int MaxQueueSize
		{
			get => (int)GetValue(MaxQueueSizeProperty);
			set => SetValue(MaxQueueSizeProperty, value);
		}

		#endregion
		#region DependencyProperty: IsShowing (read-only-like)

		public static DependencyProperty IsShowingProperty { get; } = DependencyProperty.Register(
			nameof(IsShowing),
			typeof(bool),
			typeof(SnackbarHost),
			new PropertyMetadata(false));

		public bool IsShowing
		{
			get => (bool)GetValue(IsShowingProperty);
			private set => SetValue(IsShowingProperty, value);
		}

		#endregion

		#region AttachedProperty: Host

		public static DependencyProperty HostProperty { get; } = DependencyProperty.RegisterAttached(
			"Host",
			typeof(SnackbarHost),
			typeof(SnackbarHost),
			new PropertyMetadata(default(SnackbarHost)));

		public static SnackbarHost? GetHost(DependencyObject obj) => (SnackbarHost?)obj.GetValue(HostProperty);
		public static void SetHost(DependencyObject obj, SnackbarHost? value) => obj.SetValue(HostProperty, value);

		#endregion
	}
}
```

### `src/Uno.Toolkit.UI/Controls/Snackbar/SnackbarHost.cs` (with bug fixes #3 and #4)

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

#if IS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#endif

namespace Uno.Toolkit.UI
{
	[TemplatePart(Name = TemplateParts.SnackbarPresenterName, Type = typeof(ContentPresenter))]
	[TemplateVisualState(GroupName = VisualStateNames.GroupName, Name = VisualStateNames.Hidden)]
	[TemplateVisualState(GroupName = VisualStateNames.GroupName, Name = VisualStateNames.Visible)]
	public partial class SnackbarHost : ContentControl
	{
		internal static class TemplateParts
		{
			public const string SnackbarPresenterName = "PART_SnackbarPresenter";
		}

		private class VisualStateNames
		{
			public const string GroupName = "DisplayStates";
			public const string Hidden = nameof(Hidden);
			public const string Visible = nameof(Visible);
		}

		private const double SwipeDismissThresholdRatio = 1.0 / 3;
		private static readonly TimeSpan EnterAnimationDuration = TimeSpan.FromMilliseconds(250);
		private static readonly TimeSpan ExitAnimationDuration = TimeSpan.FromMilliseconds(200);

		private ContentPresenter? _snackbarPresenter;
		private DispatcherCompat _dispatcher;
		private Storyboard _enterStoryboard = new Storyboard();
		private Storyboard _exitStoryboard = new Storyboard();
		private TranslateTransform? _translateTransform;

		private readonly Queue<PendingSnackbar> _queue = new();
		private PendingSnackbar? _current;
		private CancellationTokenSource? _autoDismissCts;
		private bool _isReady;
		private bool _isAnimating;
		private bool _isSwiping;

		internal Storyboard EnterStoryboard => _enterStoryboard;
		internal Storyboard ExitStoryboard => _exitStoryboard;

		public event EventHandler<SnackbarItem>? SnackbarOpened;
		public event EventHandler<SnackbarItem>? SnackbarClosed;

		public SnackbarHost()
		{
			DefaultStyleKey = typeof(SnackbarHost);
			_dispatcher = this.GetDispatcherCompat();
			Unloaded += OnUnloaded; // Bug fix #4
		}

		// Bug fix #4: Complete pending tasks on unload
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			CancelAutoDismissTimer();
			StopRunningAnimations();

			if (_current != null)
			{
				_current.CompletionSource.TrySetResult(SnackbarDismissReason.Dismiss);
				_current = null;
			}

			while (_queue.TryDequeue(out var pending))
			{
				pending.CompletionSource.TrySetResult(SnackbarDismissReason.Replaced);
			}

			IsShowing = false;
			_isReady = false;
		}

		protected override void OnApplyTemplate()
		{
			StopRunningAnimations();
			base.OnApplyTemplate();

			_snackbarPresenter = GetTemplateChild(TemplateParts.SnackbarPresenterName) as ContentPresenter;

			if (_snackbarPresenter != null)
			{
				_snackbarPresenter.RenderTransform = _translateTransform = new TranslateTransform();
				SetupAnimations();
			}

			_isReady = true;

			// Bug fix #3: Process any items queued before template was applied
			if (_queue.Count > 0)
			{
				ProcessQueue();
			}
		}

		#region Public API

		public Task<SnackbarDismissReason> ShowAsync(SnackbarItem item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var pending = new PendingSnackbar(item);
			EnqueueItem(pending);
			ProcessQueue();

			return pending.CompletionSource.Task;
		}

		public Task<SnackbarDismissReason> ShowAsync(
			string message,
			string? actionLabel = null,
			ICommand? actionCommand = null,
			TimeSpan? duration = null)
		{
			return ShowAsync(new SnackbarItem
			{
				Message = message,
				ActionLabel = actionLabel,
				ActionCommand = actionCommand,
				Duration = duration,
			});
		}

		public void Dismiss()
		{
			DismissCurrent(SnackbarDismissReason.Dismiss);
		}

		#endregion

		#region Queue Management

		private void EnqueueItem(PendingSnackbar pending)
		{
			_queue.Enqueue(pending);

			while (_queue.Count > MaxQueueSize)
			{
				if (_queue.TryDequeue(out var dropped))
				{
					dropped.CompletionSource.TrySetResult(SnackbarDismissReason.Replaced);
				}
			}
		}

		private void ProcessQueue()
		{
			if (_isAnimating || !_isReady) return;

			if (_current != null)
			{
				DismissCurrent(SnackbarDismissReason.Replaced);
				return;
			}

			if (!_queue.TryDequeue(out var next)) return;

			_current = next;
			ShowCurrentSnackbar();
		}

		#endregion

		#region Display Logic

		private void ShowCurrentSnackbar()
		{
			if (_current == null || _snackbarPresenter == null) return;

			var item = _current.Item;

			var snackbar = new Snackbar
			{
				Message = item.Message,
				ActionLabel = item.ActionLabel ?? string.Empty,
				ActionCommand = item.ActionCommand,
				ActionCommandParameter = item.ActionCommandParameter,
				ShowDismissButton = item.ShowDismissButton,
				IsActionOnNewLine = item.IsActionOnNewLine,
			};

			if (SnackbarStyle != null)
			{
				snackbar.Style = SnackbarStyle;
			}

			snackbar.ActionClicked += OnSnackbarActionClicked;
			snackbar.DismissClicked += OnSnackbarDismissClicked;

			if (IsSwipeToDismissEnabled)
			{
				_snackbarPresenter.ManipulationMode = ManipulationModes.TranslateX;
				_snackbarPresenter.ManipulationDelta += OnManipulationDelta;
				_snackbarPresenter.ManipulationCompleted += OnManipulationCompleted;
			}

			_snackbarPresenter.Content = snackbar;
			_snackbarPresenter.Visibility = Visibility.Visible;
			IsShowing = true;

			PlayEnterAnimation(() =>
			{
				SnackbarOpened?.Invoke(this, item);
				StartAutoDismissTimer(item);
			});
		}

		private void DismissCurrent(SnackbarDismissReason reason)
		{
			if (_current == null) return;

			CancelAutoDismissTimer();
			StopRunningAnimations();

			var current = _current;
			_current = null;

			PlayExitAnimation(() =>
			{
				CleanupSnackbar();
				current.CompletionSource.TrySetResult(reason);
				SnackbarClosed?.Invoke(this, current.Item);

				_dispatcher.Invoke(() => ProcessQueue());
			});
		}

		private void CleanupSnackbar()
		{
			if (_snackbarPresenter != null)
			{
				if (_snackbarPresenter.Content is Snackbar snackbar)
				{
					snackbar.ActionClicked -= OnSnackbarActionClicked;
					snackbar.DismissClicked -= OnSnackbarDismissClicked;
				}

				_snackbarPresenter.ManipulationDelta -= OnManipulationDelta;
				_snackbarPresenter.ManipulationCompleted -= OnManipulationCompleted;
				_snackbarPresenter.Content = null;
				_snackbarPresenter.Visibility = Visibility.Collapsed;
			}

			IsShowing = false;

			if (_translateTransform != null)
			{
				_translateTransform.X = 0;
				_translateTransform.Y = 0;
			}
		}

		#endregion

		#region Auto-Dismiss Timer

		private void StartAutoDismissTimer(SnackbarItem item)
		{
			var duration = item.Duration ?? DefaultDuration;

			if (duration == SnackbarDuration.Indefinite || duration == TimeSpan.MaxValue)
			{
				return;
			}

			CancelAutoDismissTimer();
			_autoDismissCts = new CancellationTokenSource();
			var token = _autoDismissCts.Token;

			_ = Task.Delay(duration, token).ContinueWith(t =>
			{
				if (!t.IsCanceled)
				{
					_dispatcher.Invoke(() => DismissCurrent(SnackbarDismissReason.Timeout));
				}
			}, TaskScheduler.Default);
		}

		private void CancelAutoDismissTimer()
		{
			_autoDismissCts?.Cancel();
			_autoDismissCts?.Dispose();
			_autoDismissCts = null;
		}

		#endregion

		#region Animations

		private void SetupAnimations()
		{
			if (_translateTransform == null) return;

			_enterStoryboard = new Storyboard();
			var enterTranslateY = new DoubleAnimation
			{
				From = 80, To = 0,
				Duration = new Duration(EnterAnimationDuration),
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
			};
			Storyboard.SetTarget(enterTranslateY, _translateTransform);
			Storyboard.SetTargetProperty(enterTranslateY, nameof(TranslateTransform.Y));
			_enterStoryboard.Children.Add(enterTranslateY);

			var enterOpacity = new DoubleAnimation
			{
				From = 0, To = 1,
				Duration = new Duration(TimeSpan.FromMilliseconds(150)),
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
			};
			Storyboard.SetTarget(enterOpacity, _snackbarPresenter!);
			Storyboard.SetTargetProperty(enterOpacity, nameof(UIElement.Opacity));
			_enterStoryboard.Children.Add(enterOpacity);

			_exitStoryboard = new Storyboard();
			var exitTranslateY = new DoubleAnimation
			{
				From = 0, To = 80,
				Duration = new Duration(ExitAnimationDuration),
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
			};
			Storyboard.SetTarget(exitTranslateY, _translateTransform);
			Storyboard.SetTargetProperty(exitTranslateY, nameof(TranslateTransform.Y));
			_exitStoryboard.Children.Add(exitTranslateY);

			var exitOpacity = new DoubleAnimation
			{
				From = 1, To = 0,
				Duration = new Duration(TimeSpan.FromMilliseconds(100)),
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
			};
			Storyboard.SetTarget(exitOpacity, _snackbarPresenter);
			Storyboard.SetTargetProperty(exitOpacity, nameof(UIElement.Opacity));
			_exitStoryboard.Children.Add(exitOpacity);
		}

		private void StopRunningAnimations()
		{
			StopStoryboard(_enterStoryboard);
			StopStoryboard(_exitStoryboard);
			_isAnimating = false;
		}

		private void StopStoryboard(Storyboard storyboard)
		{
			if (storyboard.GetCurrentState() != ClockState.Stopped)
			{
				storyboard.Pause();
				var currentY = _translateTransform?.Y ?? 0;
				var currentOpacity = _snackbarPresenter?.Opacity ?? 1;

				storyboard.Stop();

				if (_translateTransform != null) _translateTransform.Y = currentY;
				if (_snackbarPresenter != null) _snackbarPresenter.Opacity = currentOpacity;
			}
		}

		private void PlayEnterAnimation(Action? onCompleted = null)
		{
			_isAnimating = true;
			_enterStoryboard.Completed += OnEnterCompleted;
			_enterStoryboard.Begin();

			void OnEnterCompleted(object? sender, object e)
			{
				_enterStoryboard.Completed -= OnEnterCompleted;
				_isAnimating = false;
				onCompleted?.Invoke();
			}
		}

		private void PlayExitAnimation(Action? onCompleted = null)
		{
			_isAnimating = true;
			_exitStoryboard.Completed += OnExitCompleted;
			_exitStoryboard.Begin();

			void OnExitCompleted(object? sender, object e)
			{
				_exitStoryboard.Completed -= OnExitCompleted;
				_isAnimating = false;
				onCompleted?.Invoke();
			}
		}

		#endregion

		#region Swipe to Dismiss

		private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			if (_translateTransform == null) return;

			_translateTransform.X += e.Delta.Translation.X;
			_isSwiping = true;

			if (_snackbarPresenter != null)
			{
				var presenterWidth = _snackbarPresenter.ActualWidth > 0 ? _snackbarPresenter.ActualWidth : 300;
				var progress = Math.Abs(_translateTransform.X) / presenterWidth;
				_snackbarPresenter.Opacity = Math.Max(0, 1 - progress);
			}
		}

		private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			if (_translateTransform == null || !_isSwiping) return;

			_isSwiping = false;
			var threshold = (_snackbarPresenter?.ActualWidth ?? 300) * SwipeDismissThresholdRatio;

			if (Math.Abs(_translateTransform.X) > threshold)
			{
				DismissCurrent(SnackbarDismissReason.Dismiss);
			}
			else
			{
				SnapBackTranslateX();
			}
		}

		private void SnapBackTranslateX()
		{
			if (_translateTransform == null) return;

			var snapBack = new DoubleAnimation
			{
				To = 0,
				Duration = new Duration(TimeSpan.FromMilliseconds(150)),
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
			};
			Storyboard.SetTarget(snapBack, _translateTransform);
			Storyboard.SetTargetProperty(snapBack, nameof(TranslateTransform.X));

			var opacityRestore = new DoubleAnimation
			{
				To = 1,
				Duration = new Duration(TimeSpan.FromMilliseconds(150)),
			};
			Storyboard.SetTarget(opacityRestore, _snackbarPresenter!);
			Storyboard.SetTargetProperty(opacityRestore, nameof(UIElement.Opacity));

			var sb = new Storyboard();
			sb.Children.Add(snapBack);
			sb.Children.Add(opacityRestore);
			sb.Begin();
		}

		#endregion

		#region Event Handlers

		private void OnSnackbarActionClicked(object? sender, EventArgs e)
		{
			DismissCurrent(SnackbarDismissReason.Action);
		}

		private void OnSnackbarDismissClicked(object? sender, EventArgs e)
		{
			DismissCurrent(SnackbarDismissReason.Dismiss);
		}

		#endregion

		#region Internal Types

		private class PendingSnackbar
		{
			public SnackbarItem Item { get; }
			public TaskCompletionSource<SnackbarDismissReason> CompletionSource { get; } = new();

			public PendingSnackbar(SnackbarItem item)
			{
				Item = item;
			}
		}

		#endregion
	}
}
```

### XAML Templates

The base `Snackbar.xaml`, `SnackbarHost.xaml`, Material `Snackbar.xaml`, and Cupertino `Snackbar.xaml` are too large to include inline but are already in the repo at their correct paths. See the File Inventory section for locations.

### Sample Page and Tests

`SnackbarSamplePage.xaml`, `SnackbarSamplePage.xaml.cs`, and `SnackbarTests.cs` are already in the repo at their correct paths. See the File Inventory section for locations.
