using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Uno.Toolkit.UI;

public class SlideTransition : CarouselTransitionBase
{
    protected override void BuildAnimations(Storyboard sb, UIElement? from, UIElement to, bool forward)
    {
        var duration = new Duration(TimeSpan.FromMilliseconds(400));
        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };
        var distance = to is FrameworkElement fe && fe.ActualWidth > 0 ? fe.ActualWidth : 800;

        // Incoming: slide in from off-screen
        EnsureTranslateTransform(to);
        var inStart = forward ? distance : -distance;
        if (to.RenderTransform is TranslateTransform toTt) toTt.X = inStart;
        to.Opacity = 1.0;
        Canvas.SetZIndex(to, 1);

        AddAnimation(sb, to.RenderTransform, nameof(TranslateTransform.X),
            inStart, 0, duration, easing, dependent: true);

        // Outgoing: slide out opposite side
        if (from != null)
        {
            EnsureTranslateTransform(from);
            from.Opacity = 1.0;
            Canvas.SetZIndex(from, 0);

            AddAnimation(sb, from.RenderTransform, nameof(TranslateTransform.X),
                0, forward ? -distance : distance, duration, easing, dependent: true);
        }
    }

    protected override void Finalize(UIElement? from, UIElement? to)
    {
        base.Finalize(from, to);
        if (from?.RenderTransform is TranslateTransform fromTt) fromTt.X = 0;
        if (to?.RenderTransform is TranslateTransform toTt) toTt.X = 0;
    }
}
