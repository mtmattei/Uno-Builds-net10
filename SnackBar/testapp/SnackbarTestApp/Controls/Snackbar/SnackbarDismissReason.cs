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
