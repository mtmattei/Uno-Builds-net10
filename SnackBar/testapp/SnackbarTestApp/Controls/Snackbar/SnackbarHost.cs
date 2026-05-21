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
		private static readonly TimeSpan EnterAnimationDuration = TimeSpan.FromMilliseconds(150);
		private static readonly TimeSpan ExitAnimationDuration = TimeSpan.FromMilliseconds(75);

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
			Unloaded += OnUnloaded;
		}

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

			// M3 spec: Enter 150ms Emphasized Decelerate, Exit 75ms Emphasized Accelerate
			_enterStoryboard = new Storyboard();
			var enterTranslateY = new DoubleAnimation
			{
				From = 80, To = 0,
				Duration = new Duration(EnterAnimationDuration),
				EasingFunction = new ExponentialEase { Exponent = 4.5, EasingMode = EasingMode.EaseOut },
			};
			Storyboard.SetTarget(enterTranslateY, _translateTransform);
			Storyboard.SetTargetProperty(enterTranslateY, nameof(TranslateTransform.Y));
			_enterStoryboard.Children.Add(enterTranslateY);

			var enterOpacity = new DoubleAnimation
			{
				From = 0, To = 1,
				Duration = new Duration(TimeSpan.FromMilliseconds(100)),
				EasingFunction = new ExponentialEase { Exponent = 4.5, EasingMode = EasingMode.EaseOut },
			};
			Storyboard.SetTarget(enterOpacity, _snackbarPresenter!);
			Storyboard.SetTargetProperty(enterOpacity, nameof(UIElement.Opacity));
			_enterStoryboard.Children.Add(enterOpacity);

			_exitStoryboard = new Storyboard();
			var exitTranslateY = new DoubleAnimation
			{
				From = 0, To = 80,
				Duration = new Duration(ExitAnimationDuration),
				EasingFunction = new ExponentialEase { Exponent = 4.5, EasingMode = EasingMode.EaseIn },
			};
			Storyboard.SetTarget(exitTranslateY, _translateTransform);
			Storyboard.SetTargetProperty(exitTranslateY, nameof(TranslateTransform.Y));
			_exitStoryboard.Children.Add(exitTranslateY);

			var exitOpacity = new DoubleAnimation
			{
				From = 1, To = 0,
				Duration = new Duration(TimeSpan.FromMilliseconds(50)),
				EasingFunction = new ExponentialEase { Exponent = 4.5, EasingMode = EasingMode.EaseIn },
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
