using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Uno.Toolkit.UI;

public class FadeTransition : CarouselTransitionBase
{
    protected override void BuildAnimations(Storyboard sb, UIElement? from, UIElement to, bool forward)
    {
        var duration = new Duration(TimeSpan.FromMilliseconds(500));
        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

        to.Opacity = 0;
        Canvas.SetZIndex(to, 1);
        AddAnimation(sb, to, nameof(UIElement.Opacity), 0, 1, duration, easing);

        if (from != null)
        {
            from.Opacity = 1.0;
            Canvas.SetZIndex(from, 0);
            AddAnimation(sb, from, nameof(UIElement.Opacity), 1, 0, duration, easing);
        }
    }
}
