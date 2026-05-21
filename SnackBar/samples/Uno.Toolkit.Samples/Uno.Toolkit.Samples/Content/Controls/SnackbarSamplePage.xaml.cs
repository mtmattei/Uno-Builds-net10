using System;
using System.Windows.Input;
using Uno.Toolkit.Samples;
using Uno.Toolkit.UI;

#if IS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Uno.Toolkit.Samples.Content.Controls
{
	[SamplePage(SampleCategory.Controls, "Snackbar")]
	public sealed partial class SnackbarSamplePage : Page
	{
		private SnackbarHost? _snackbarHost;

		public SnackbarSamplePage()
		{
			this.InitializeComponent();
			this.Loaded += OnPageLoaded;
		}

		private void OnPageLoaded(object sender, RoutedEventArgs e)
		{
			_snackbarHost = SamplePageLayout.GetSampleChild<SnackbarHost>(
				Design.Agnostic, "SampleSnackbarHost");
		}

		private async void OnShowBasicClick(object sender, RoutedEventArgs e)
		{
			if (_snackbarHost == null) return;

			var reason = await _snackbarHost.ShowAsync(new SnackbarItem
			{
				Message = "Photo saved to gallery",
			});

			UpdateResult(reason);
		}

		private async void OnShowActionClick(object sender, RoutedEventArgs e)
		{
			if (_snackbarHost == null) return;

			var reason = await _snackbarHost.ShowAsync(new SnackbarItem
			{
				Message = "Item deleted",
				ActionLabel = "Undo",
			});

			UpdateResult(reason);
		}

		private async void OnShowActionDismissClick(object sender, RoutedEventArgs e)
		{
			if (_snackbarHost == null) return;

			var reason = await _snackbarHost.ShowAsync(new SnackbarItem
			{
				Message = "Connection restored",
				ActionLabel = "Retry",
				ShowDismissButton = true,
			});

			UpdateResult(reason);
		}

		private async void OnShowMultiLineClick(object sender, RoutedEventArgs e)
		{
			if (_snackbarHost == null) return;

			var reason = await _snackbarHost.ShowAsync(new SnackbarItem
			{
				Message = "This item already has the label \"travel\". You can add a new label or use the search to find existing ones.",
				ActionLabel = "Got It",
				ShowDismissButton = true,
				IsActionOnNewLine = true,
			});

			UpdateResult(reason);
		}

		private async void OnShowLongClick(object sender, RoutedEventArgs e)
		{
			if (_snackbarHost == null) return;

			var reason = await _snackbarHost.ShowAsync(new SnackbarItem
			{
				Message = "This message will stay for 7 seconds",
				Duration = SnackbarDuration.Long,
			});

			UpdateResult(reason);
		}

		private async void OnShowIndefiniteClick(object sender, RoutedEventArgs e)
		{
			if (_snackbarHost == null) return;

			var reason = await _snackbarHost.ShowAsync(new SnackbarItem
			{
				Message = "This won't go away on its own",
				ShowDismissButton = true,
				Duration = SnackbarDuration.Indefinite,
			});

			UpdateResult(reason);
		}

		private async void OnShowQueueClick(object sender, RoutedEventArgs e)
		{
			if (_snackbarHost == null) return;

			// Fire all three without awaiting; they queue automatically
			var t1 = _snackbarHost.ShowAsync(new SnackbarItem { Message = "Message 1 of 3" });
			var t2 = _snackbarHost.ShowAsync(new SnackbarItem { Message = "Message 2 of 3" });
			var t3 = _snackbarHost.ShowAsync(new SnackbarItem { Message = "Message 3 of 3" });

			// Show the last result
			var reason = await t3;
			UpdateResult(reason);
		}

		private void UpdateResult(SnackbarDismissReason reason)
		{
			var resultText = SamplePageLayout.GetSampleChild<TextBlock>(
				Design.Agnostic, "ResultText");

			if (resultText != null)
			{
				resultText.Text = $"Last dismiss reason: {reason}";
			}
		}
	}
}
