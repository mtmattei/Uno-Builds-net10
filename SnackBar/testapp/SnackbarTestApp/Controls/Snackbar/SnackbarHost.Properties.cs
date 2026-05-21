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
