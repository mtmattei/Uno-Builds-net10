using System;
using Microsoft.UI.Xaml;

namespace Uno.Toolkit.UI;

/// <summary>
/// Defines a pluggable transition animation for carousel item changes.
/// </summary>
public interface ICarouselTransition
{
    /// <summary>
    /// Runs the transition between two items. The implementation must call
    /// <paramref name="onCompleted"/> when the animation finishes so the
    /// carousel can finalize visibility state.
    /// </summary>
    /// <param name="from">The element being navigated away from. May be null on first load.</param>
    /// <param name="to">The element being navigated to.</param>
    /// <param name="forward">True if navigating forward (next), false if backward (previous).</param>
    /// <param name="onCompleted">Callback to invoke when the animation completes.</param>
    void Run(UIElement? from, UIElement to, bool forward, Action onCompleted);

    /// <summary>
    /// Immediately cancels any running transition and resets visual state.
    /// </summary>
    void Cancel();
}
