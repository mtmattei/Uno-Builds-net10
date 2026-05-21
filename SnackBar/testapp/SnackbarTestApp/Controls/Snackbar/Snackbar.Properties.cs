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
