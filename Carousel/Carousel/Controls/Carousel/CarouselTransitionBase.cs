using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Uno.Toolkit.UI;

/// <summary>
/// Shared base for carousel transitions. Manages storyboard lifecycle and cleanup.
/// </summary>
public abstract class CarouselTransitionBase : ICarouselTransition
{
    private Storyboard? _storyboard;
    private UIElement? _from;
    private UIElement? _to;

    public void Run(UIElement? from, UIElement to, bool forward, Action onCompleted)
    {
        Cancel();
        _from = from;
        _to = to;

        var sb = new Storyboard();
        BuildAnimations(sb, from, to, forward);
        _storyboard = sb;

        sb.Completed += (_, _) =>
        {
            Finalize(from, to);
            _storyboard = null;
            _from = null;
            _to = null;
            onCompleted();
        };
        sb.Begin();
    }

    public void Cancel()
    {
        if (_storyboard != null)
        {
            _storyboard.Stop();
            Finalize(_from, _to);
            _storyboard = null;
            _from = null;
            _to = null;
        }
    }

    protected abstract void BuildAnimations(Storyboard sb, UIElement? from, UIElement to, bool forward);

    protected virtual void Finalize(UIElement? from, UIElement? to)
    {
        if (from != null)
        {
            from.Opacity = 0;
            Canvas.SetZIndex(from, 0);
        }
        if (to != null)
        {
            to.Opacity = 1.0;
            Canvas.SetZIndex(to, 1);
        }
    }

    protected static void AddAnimation(Storyboard sb, DependencyObject target, string property,
        double from, double to, Duration duration, EasingFunctionBase? easing, bool dependent = false)
    {
        var anim = new DoubleAnimation
        {
            From = from, To = to, Duration = duration,
            EasingFunction = easing, EnableDependentAnimation = dependent
        };
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, property);
        sb.Children.Add(anim);
    }

    protected static void EnsureTranslateTransform(UIElement element)
    {
        if (element.RenderTransform is not TranslateTransform)
            element.RenderTransform = new TranslateTransform();
    }
}
