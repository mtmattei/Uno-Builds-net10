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
