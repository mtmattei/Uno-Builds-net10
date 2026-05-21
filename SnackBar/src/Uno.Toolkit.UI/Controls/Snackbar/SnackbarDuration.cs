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
