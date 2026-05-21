using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Toolkit.UI;

public partial class Carousel
{
    public static DependencyProperty ItemsSourceProperty { get; } = DependencyProperty.Register(
        nameof(ItemsSource), typeof(object), typeof(Carousel),
        new PropertyMetadata(null, (s, e) => ((Carousel)s).OnItemsSourceChanged(e)));

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static DependencyProperty ItemTemplateProperty { get; } = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(Carousel),
        new PropertyMetadata(null, (s, e) => ((Carousel)s).RebuildItems()));

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static DependencyProperty SelectedIndexProperty { get; } = DependencyProperty.Register(
        nameof(SelectedIndex), typeof(int), typeof(Carousel),
        new PropertyMetadata(-1, (s, e) => ((Carousel)s).OnSelectedIndexChanged(e)));

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static DependencyProperty SelectedItemProperty { get; } = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(Carousel),
        new PropertyMetadata(null, (s, e) => ((Carousel)s).OnSelectedItemChanged(e)));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static DependencyProperty ItemTransitionProperty { get; } = DependencyProperty.Register(
        nameof(ItemTransition), typeof(ICarouselTransition), typeof(Carousel),
        new PropertyMetadata(null, (s, e) =>
        {
            var c = (Carousel)s;
            (e.OldValue as ICarouselTransition)?.Cancel();
            c._transition = e.NewValue as ICarouselTransition ?? new SlideTransition();
        }));

    public ICarouselTransition? ItemTransition
    {
        get => (ICarouselTransition?)GetValue(ItemTransitionProperty);
        set => SetValue(ItemTransitionProperty, value);
    }

    public static DependencyProperty OrientationProperty { get; } = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(Carousel),
        new PropertyMetadata(Orientation.Horizontal, (s, e) =>
        {
            var c = (Carousel)s;
            if (c._indicatorPanel != null) c._indicatorPanel.Orientation = c.Orientation;
            c.UpdateManipulationMode();
        }));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static DependencyProperty IsLoopingEnabledProperty { get; } = DependencyProperty.Register(
        nameof(IsLoopingEnabled), typeof(bool), typeof(Carousel),
        new PropertyMetadata(true, (s, _) => ((Carousel)s).UpdateNavigationButtonStates()));

    public bool IsLoopingEnabled
    {
        get => (bool)GetValue(IsLoopingEnabledProperty);
        set => SetValue(IsLoopingEnabledProperty, value);
    }

    public static DependencyProperty AutoPlayIntervalProperty { get; } = DependencyProperty.Register(
        nameof(AutoPlayInterval), typeof(object), typeof(Carousel),
        new PropertyMetadata(null, (s, _) => ((Carousel)s).UpdateAutoPlay()));

    public TimeSpan? AutoPlayInterval
    {
        get => GetValue(AutoPlayIntervalProperty) as TimeSpan?;
        set => SetValue(AutoPlayIntervalProperty, value);
    }

    public static DependencyProperty IsAutoPlayEnabledProperty { get; } = DependencyProperty.Register(
        nameof(IsAutoPlayEnabled), typeof(bool), typeof(Carousel),
        new PropertyMetadata(false, (s, _) => ((Carousel)s).UpdateAutoPlay()));

    public bool IsAutoPlayEnabled
    {
        get => (bool)GetValue(IsAutoPlayEnabledProperty);
        set => SetValue(IsAutoPlayEnabledProperty, value);
    }

    public static DependencyProperty IndicatorVisibilityProperty { get; } = DependencyProperty.Register(
        nameof(IndicatorVisibility), typeof(Visibility), typeof(Carousel),
        new PropertyMetadata(Visibility.Visible));

    public Visibility IndicatorVisibility
    {
        get => (Visibility)GetValue(IndicatorVisibilityProperty);
        set => SetValue(IndicatorVisibilityProperty, value);
    }

    public static DependencyProperty IndicatorStyleProperty { get; } = DependencyProperty.Register(
        nameof(IndicatorStyle), typeof(Style), typeof(Carousel),
        new PropertyMetadata(null));

    public Style? IndicatorStyle
    {
        get => (Style?)GetValue(IndicatorStyleProperty);
        set => SetValue(IndicatorStyleProperty, value);
    }
}
