using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Uno.Toolkit.UI
{
	/// <summary>
	/// Lightweight shim that wraps DispatcherQueue to provide the same API
	/// as the internal DispatcherCompat helper in the Uno Toolkit NuGet.
	/// </summary>
	internal readonly struct DispatcherCompat
	{
		private readonly DispatcherQueue _queue;

		public DispatcherCompat(DispatcherQueue queue)
		{
			_queue = queue;
		}

		public void Invoke(DispatcherQueueHandler action)
		{
			if (_queue.HasThreadAccess)
			{
				action();
			}
			else
			{
				_queue.TryEnqueue(action);
			}
		}
	}

	internal static class DispatcherCompatExtensions
	{
		public static DispatcherCompat GetDispatcherCompat(this DependencyObject obj)
		{
			return new DispatcherCompat(obj.DispatcherQueue);
		}
	}
}
