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
