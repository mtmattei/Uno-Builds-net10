using System.Collections;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Uno.Toolkit.UI;

[TemplatePart(Name = PART_ItemsPanel, Type = typeof(CarouselPanel))]
[TemplatePart(Name = PART_IndicatorPanel, Type = typeof(CarouselIndicator))]
[TemplatePart(Name = PART_PreviousButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PART_NextButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PART_ClipGrid, Type = typeof(Grid))]
public partial class Carousel : Control
{
    private const string PART_ItemsPanel = "PART_ItemsPanel";
    private const string PART_IndicatorPanel = "PART_IndicatorPanel";
    private const string PART_PreviousButton = "PART_PreviousButton";
    private const string PART_NextButton = "PART_NextButton";
    private const string PART_ClipGrid = "PART_ClipGrid";

    private CarouselPanel? _itemsPanel;
    private CarouselIndicator? _indicatorPanel;
    private ButtonBase? _previousButton;
    private ButtonBase? _nextButton;
    private Grid? _clipGrid;

    internal ICarouselTransition _transition = new SlideTransition();
    private DispatcherTimer? _autoPlayTimer;
    private bool _isSynchronizingSelection;
    private bool _isTransitioning;
    private bool _isManipulating;
    private int _selectedIndex = -1;
    private int _itemCount;
    private IList? _itemsSource;

    // Swipe gesture state
    private UIElement? _peekElement;
    private int _peekIndex = -1;

    public Carousel()
    {
        DefaultStyleKey = typeof(Carousel);
        Loaded += (_, _) => UpdateAutoPlay();
        Unloaded += OnUnloaded;
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new CarouselAutomationPeer(this);

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Unhook old
        if (_previousButton != null) _previousButton.Click -= OnNavButtonClick;
        if (_nextButton != null) _nextButton.Click -= OnNavButtonClick;
        if (_clipGrid != null)
        {
            _clipGrid.SizeChanged -= OnClipGridSizeChanged;
            _clipGrid.ManipulationStarted -= OnManipulationStarted;
            _clipGrid.ManipulationDelta -= OnManipulationDelta;
            _clipGrid.ManipulationCompleted -= OnManipulationCompleted;
        }

        _clipGrid = GetTemplateChild(PART_ClipGrid) as Grid;
        _itemsPanel = GetTemplateChild(PART_ItemsPanel) as CarouselPanel;
        _indicatorPanel = GetTemplateChild(PART_IndicatorPanel) as CarouselIndicator;
        _previousButton = GetTemplateChild(PART_PreviousButton) as ButtonBase;
        _nextButton = GetTemplateChild(PART_NextButton) as ButtonBase;

        // Hook new
        if (_previousButton != null) _previousButton.Click += OnNavButtonClick;
        if (_nextButton != null) _nextButton.Click += OnNavButtonClick;
        if (_clipGrid != null)
        {
            _clipGrid.SizeChanged += OnClipGridSizeChanged;
            UpdateManipulationMode();
            _clipGrid.ManipulationStarted += OnManipulationStarted;
            _clipGrid.ManipulationDelta += OnManipulationDelta;
            _clipGrid.ManipulationCompleted += OnManipulationCompleted;
        }

        if (_indicatorPanel != null)
        {
            _indicatorPanel.Orientation = Orientation;
            // Resolve theme brushes from ThemeDictionaries via the app resource tree
            if (Application.Current.Resources.TryGetValue("CarouselIndicatorActiveBrush", out var ab) && ab is Brush activeBrush)
                _indicatorPanel.ActiveBrush = activeBrush;
            if (Application.Current.Resources.TryGetValue("CarouselIndicatorInactiveBrush", out var ib) && ib is Brush inactiveBrush)
                _indicatorPanel.InactiveBrush = inactiveBrush;
        }

        RebuildItems();
        UpdateAutoPlay();
    }

    private void OnClipGridSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_clipGrid != null)
            _clipGrid.Clip = new RectangleGeometry
            {
                Rect = new Windows.Foundation.Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopAutoPlay();
        CancelTransition();
        if (_clipGrid != null)
        {
            _clipGrid.ManipulationStarted -= OnManipulationStarted;
            _clipGrid.ManipulationDelta -= OnManipulationDelta;
            _clipGrid.ManipulationCompleted -= OnManipulationCompleted;
        }
    }

    private void OnNavButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender == _previousButton) Previous();
        else Next();
    }

    #region Navigation

    public void Next()
    {
        if (_isTransitioning || _itemCount == 0) return;
        var next = WrapIndex(_selectedIndex + 1);
        if (next >= 0) GoTo(next);
    }

    public void Previous()
    {
        if (_isTransitioning || _itemCount == 0) return;
        var prev = WrapIndex(_selectedIndex - 1);
        if (prev >= 0) GoTo(prev);
    }

    public void GoTo(int index, bool animate = true)
    {
        if (_itemCount == 0 || index < 0 || index >= _itemCount || index == _selectedIndex) return;

        CancelTransition();

        var previousItem = GetItemAt(_selectedIndex);
        var newItem = GetItemAt(index);

        var changingArgs = new CarouselSelectionChangingEventArgs(previousItem, newItem);
        SelectionChanging?.Invoke(this, changingArgs);
        if (changingArgs.Cancel) return;

        var previousIndex = _selectedIndex;
        var forward = index > previousIndex
            || (IsLoopingEnabled && previousIndex == _itemCount - 1 && index == 0);

        ApplySelection(index, newItem);
        UpdateNavigationButtonStates();

        var fromElement = GetChildAt(previousIndex);
        var toElement = GetChildAt(index);

        if (toElement != null && animate && previousIndex >= 0)
        {
            _isTransitioning = true;
            _transition.Run(fromElement, toElement, forward, () =>
            {
                _isTransitioning = false;
                SetItemVisibility(index);
            });
        }
        else
        {
            SetItemVisibility(index);
        }

        SelectionChanged?.Invoke(this, new CarouselSelectionChangedEventArgs(
            previousItem, newItem, previousIndex, index));
        ResetAutoPlayTimer();
    }

    /// <summary>
    /// Wraps an index for looping. Returns -1 if out of bounds and looping is disabled.
    /// </summary>
    private int WrapIndex(int index)
    {
        if (index >= 0 && index < _itemCount) return index;
        if (!IsLoopingEnabled) return -1;
        if (index < 0) return _itemCount - 1;
        return 0;
    }

    private void ApplySelection(int index, object? item)
    {
        _isSynchronizingSelection = true;
        _selectedIndex = index;
        SelectedIndex = index;
        SelectedItem = item;
        _isSynchronizingSelection = false;
        if (_indicatorPanel != null) _indicatorPanel.SelectedIndex = index;
    }

    private void ClearSelection()
    {
        _isSynchronizingSelection = true;
        _selectedIndex = -1;
        SelectedIndex = -1;
        SelectedItem = null;
        _isSynchronizingSelection = false;
    }

    private void CancelTransition()
    {
        if (_isTransitioning)
        {
            _transition.Cancel();
            _isTransitioning = false;
        }
    }

    #endregion

    #region Item Management

    private object? GetItemAt(int index)
        => _itemsSource is IList list && index >= 0 && index < list.Count ? list[index] : null;

    private UIElement? GetChildAt(int index)
        => _itemsPanel != null && index >= 0 && index < _itemsPanel.Children.Count
            ? _itemsPanel.Children[index] : null;

    private void SetItemVisibility(int visibleIndex)
    {
        if (_itemsPanel == null) return;
        for (int i = 0; i < _itemsPanel.Children.Count; i++)
        {
            var child = _itemsPanel.Children[i];
            var visible = i == visibleIndex;
            child.Opacity = visible ? 1.0 : 0;
            Canvas.SetZIndex(child, visible ? 1 : 0);
            if (child.RenderTransform is TranslateTransform tt)
            {
                tt.X = 0;
                tt.Y = 0;
            }
        }
    }

    private void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs args)
    {
        if (!_isSynchronizingSelection && (int)args.NewValue is var idx && idx != _selectedIndex)
            GoTo(idx);
    }

    private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs args)
    {
        if (!_isSynchronizingSelection && args.NewValue != null && _itemsSource is IList list)
        {
            var index = list.IndexOf(args.NewValue);
            if (index >= 0 && index != _selectedIndex) GoTo(index);
        }
    }

    private void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
    {
        if (args.OldValue is INotifyCollectionChanged oldNcc)
            oldNcc.CollectionChanged -= OnItemsCollectionChanged;

        _itemsSource = args.NewValue as IList;
        if (_itemsSource == null && args.NewValue is IEnumerable enumerable)
        {
            var list = new System.Collections.Generic.List<object>();
            foreach (var item in enumerable) list.Add(item);
            _itemsSource = list;
        }

        if (args.NewValue is INotifyCollectionChanged newNcc)
            newNcc.CollectionChanged += OnItemsCollectionChanged;

        RebuildItems();
        ValidateSelectionBounds();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildItems();
        ValidateSelectionBounds();
    }

    private void ValidateSelectionBounds()
    {
        if (_itemCount == 0) ClearSelection();
        else if (_selectedIndex < 0 || _selectedIndex >= _itemCount) GoTo(Math.Min(_itemCount - 1, Math.Max(0, _selectedIndex)), false);
    }

    private void RebuildItems()
    {
        if (_itemsPanel == null) return;

        CancelTransition();
        _itemsPanel.Children.Clear();

        if (_itemsSource == null)
        {
            _itemCount = 0;
            if (_indicatorPanel != null)
            {
                _indicatorPanel.ItemCount = 0;
                _indicatorPanel.Visibility = Visibility.Collapsed;
            }
            UpdateNavigationButtonStates();
            return;
        }

        var template = ItemTemplate;
        foreach (var item in _itemsSource)
        {
            FrameworkElement? content = null;
            if (template != null) content = template.LoadContent() as FrameworkElement;

            if (content != null)
                content.DataContext = item;
            else
                content = new ContentPresenter
                {
                    Content = item,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch
                };

            _itemsPanel.Children.Add(content);
        }

        _itemCount = _itemsPanel.Children.Count;
        var selected = Math.Max(0, _selectedIndex);
        SetItemVisibility(selected);

        if (_indicatorPanel != null)
        {
            _indicatorPanel.ItemCount = _itemCount;
            _indicatorPanel.SelectedIndex = selected;
            _indicatorPanel.Visibility = _itemCount > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        UpdateNavigationButtonStates();
    }

    #endregion

    #region Navigation Button State

    private void UpdateManipulationMode()
    {
        if (_clipGrid != null)
            _clipGrid.ManipulationMode = Orientation == Orientation.Horizontal
                ? ManipulationModes.TranslateX | ManipulationModes.TranslateInertia
                : ManipulationModes.TranslateY | ManipulationModes.TranslateInertia;
    }

    private void UpdateNavigationButtonStates()
    {
        if (_previousButton != null)
            _previousButton.IsEnabled = _itemCount > 1 && (IsLoopingEnabled || _selectedIndex > 0);
        if (_nextButton != null)
            _nextButton.IsEnabled = _itemCount > 1 && (IsLoopingEnabled || _selectedIndex < _itemCount - 1);
    }

    #endregion

    #region Swipe Gestures

    private static void EnsureTranslateTransform(UIElement element)
    {
        if (element.RenderTransform is not TranslateTransform)
            element.RenderTransform = new TranslateTransform();
    }

    private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {
        if (_isTransitioning || _itemCount <= 1) { e.Complete(); return; }
        _isManipulating = true;
        _peekElement = null;
        _peekIndex = -1;
        _autoPlayTimer?.Stop();
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        if (!_isManipulating || _clipGrid == null) return;

        var horizontal = Orientation == Orientation.Horizontal;
        var cumulative = horizontal ? e.Cumulative.Translation.X : e.Cumulative.Translation.Y;
        var panelSize = horizontal ? _clipGrid.ActualWidth : _clipGrid.ActualHeight;
        if (panelSize <= 0) return;

        var currentElement = GetChildAt(_selectedIndex);
        if (currentElement == null) return;

        var rawTarget = cumulative < 0 ? _selectedIndex + 1 : _selectedIndex - 1;
        var targetIndex = WrapIndex(rawTarget);
        var atBoundary = targetIndex < 0;
        var translation = atBoundary ? cumulative * 0.3 : cumulative;

        // Move current item
        EnsureTranslateTransform(currentElement);
        SetTranslation(currentElement, translation, horizontal);

        // Position peek item
        if (!atBoundary)
        {
            if (_peekIndex != targetIndex)
            {
                HidePeek();
                _peekIndex = targetIndex;
                _peekElement = GetChildAt(targetIndex);
                if (_peekElement != null)
                {
                    EnsureTranslateTransform(_peekElement);
                    _peekElement.Opacity = 1;
                    Canvas.SetZIndex(_peekElement, 0);
                    Canvas.SetZIndex(currentElement, 1);
                }
            }

            if (_peekElement != null)
            {
                var peekOffset = (cumulative < 0 ? panelSize : -panelSize) + translation;
                SetTranslation(_peekElement, peekOffset, horizontal);
            }
        }
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        if (!_isManipulating || _clipGrid == null) return;
        _isManipulating = false;

        var horizontal = Orientation == Orientation.Horizontal;
        var cumulative = horizontal ? e.Cumulative.Translation.X : e.Cumulative.Translation.Y;
        var velocity = horizontal ? e.Velocities.Linear.X : e.Velocities.Linear.Y;
        var panelSize = horizontal ? _clipGrid.ActualWidth : _clipGrid.ActualHeight;

        if (panelSize <= 0) { CleanUpManipulation(); return; }

        var thresholdMet = Math.Abs(cumulative) > panelSize * 0.25 || Math.Abs(velocity) > 0.5;
        var rawTarget = cumulative < 0 ? _selectedIndex + 1 : _selectedIndex - 1;
        var targetIndex = WrapIndex(rawTarget);

        if (thresholdMet && targetIndex >= 0)
            CommitSwipeNavigation(targetIndex, cumulative < 0, panelSize, horizontal);
        else
            AnimateSnapBack(horizontal, panelSize);

        if (IsAutoPlayEnabled && _autoPlayTimer != null) _autoPlayTimer.Start();
    }

    private void CommitSwipeNavigation(int targetIndex, bool forward, double panelSize, bool horizontal)
    {
        _isTransitioning = true;
        var storyboard = new Storyboard();
        var prop = horizontal ? nameof(TranslateTransform.X) : nameof(TranslateTransform.Y);
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = new Duration(TimeSpan.FromMilliseconds(200));

        var currentElement = GetChildAt(_selectedIndex);
        if (currentElement != null)
        {
            EnsureTranslateTransform(currentElement);
            AddTranslateAnimation(storyboard, currentElement, GetTranslation(currentElement, horizontal),
                forward ? -panelSize : panelSize, duration, easing, prop);
        }

        var peekElement = GetChildAt(targetIndex);
        if (peekElement != null)
        {
            EnsureTranslateTransform(peekElement);
            AddTranslateAnimation(storyboard, peekElement, GetTranslation(peekElement, horizontal),
                0, duration, easing, prop);
        }

        var captured = targetIndex;
        storyboard.Completed += (_, _) =>
        {
            _isTransitioning = false;
            _peekElement = null;
            _peekIndex = -1;

            var previousItem = GetItemAt(_selectedIndex);
            var newItem = GetItemAt(captured);
            var previousIndex = _selectedIndex;

            ApplySelection(captured, newItem);
            SetItemVisibility(captured);
            UpdateNavigationButtonStates();

            SelectionChanged?.Invoke(this, new CarouselSelectionChangedEventArgs(
                previousItem, newItem, previousIndex, captured));
            ResetAutoPlayTimer();
        };
        storyboard.Begin();
    }

    private void AnimateSnapBack(bool horizontal, double panelSize)
    {
        _isTransitioning = true;
        var storyboard = new Storyboard();
        var prop = horizontal ? nameof(TranslateTransform.X) : nameof(TranslateTransform.Y);
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = new Duration(TimeSpan.FromMilliseconds(250));

        var currentElement = GetChildAt(_selectedIndex);
        if (currentElement != null)
        {
            EnsureTranslateTransform(currentElement);
            AddTranslateAnimation(storyboard, currentElement, GetTranslation(currentElement, horizontal),
                0, duration, easing, prop);
        }

        if (_peekElement != null && _peekIndex >= 0)
        {
            EnsureTranslateTransform(_peekElement);
            var pos = GetTranslation(_peekElement, horizontal);
            AddTranslateAnimation(storyboard, _peekElement, pos,
                pos > 0 ? panelSize : -panelSize, duration, easing, prop);
        }

        storyboard.Completed += (_, _) =>
        {
            _isTransitioning = false;
            CleanUpManipulation();
            SetItemVisibility(_selectedIndex);
        };
        storyboard.Begin();
    }

    private static void AddTranslateAnimation(Storyboard sb, UIElement element,
        double from, double to, Duration duration, EasingFunctionBase easing, string property)
    {
        var anim = new DoubleAnimation
        {
            From = from, To = to, Duration = duration,
            EasingFunction = easing, EnableDependentAnimation = true
        };
        Storyboard.SetTarget(anim, element.RenderTransform);
        Storyboard.SetTargetProperty(anim, property);
        sb.Children.Add(anim);
    }

    private static void SetTranslation(UIElement element, double value, bool horizontal)
    {
        if (element.RenderTransform is TranslateTransform tt)
        {
            if (horizontal) tt.X = value; else tt.Y = value;
        }
    }

    private static double GetTranslation(UIElement element, bool horizontal)
        => element.RenderTransform is TranslateTransform tt
            ? (horizontal ? tt.X : tt.Y) : 0;

    private void HidePeek()
    {
        if (_peekElement != null && _peekIndex != _selectedIndex)
        {
            _peekElement.Opacity = 0;
            Canvas.SetZIndex(_peekElement, 0);
        }
    }

    private void CleanUpManipulation()
    {
        HidePeek();
        if (_peekElement?.RenderTransform is TranslateTransform tt) { tt.X = 0; tt.Y = 0; }
        _peekElement = null;
        _peekIndex = -1;
    }

    #endregion

    #region Keyboard Navigation

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        switch (e.Key)
        {
            case Windows.System.VirtualKey.Left when Orientation == Orientation.Horizontal:
            case Windows.System.VirtualKey.Up when Orientation == Orientation.Vertical:
                Previous(); e.Handled = true; break;
            case Windows.System.VirtualKey.Right when Orientation == Orientation.Horizontal:
            case Windows.System.VirtualKey.Down when Orientation == Orientation.Vertical:
                Next(); e.Handled = true; break;
            case Windows.System.VirtualKey.Home:
                GoTo(0); e.Handled = true; break;
            case Windows.System.VirtualKey.End:
                if (_itemCount > 0) GoTo(_itemCount - 1); e.Handled = true; break;
        }
    }

    #endregion

    #region Auto-Play

    private void UpdateAutoPlay()
    {
        if (IsAutoPlayEnabled && AutoPlayInterval is TimeSpan interval && interval > TimeSpan.Zero)
        {
            if (_autoPlayTimer == null)
            {
                _autoPlayTimer = new DispatcherTimer();
                _autoPlayTimer.Tick += (_, _) => Next();
            }
            _autoPlayTimer.Interval = interval;
            _autoPlayTimer.Start();
        }
        else
        {
            StopAutoPlay();
        }
    }

    private void StopAutoPlay()
    {
        if (_autoPlayTimer != null)
        {
            _autoPlayTimer.Stop();
            _autoPlayTimer = null;
        }
    }

    private void ResetAutoPlayTimer()
    {
        if (_autoPlayTimer is { IsEnabled: true })
        {
            _autoPlayTimer.Stop();
            _autoPlayTimer.Start();
        }
    }

    #endregion

    #region Pointer Interaction (pause auto-play on hover)

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);
        if (e.Pointer.PointerDeviceType.Equals(Microsoft.UI.Input.PointerDeviceType.Mouse))
            _autoPlayTimer?.Stop();
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        if (e.Pointer.PointerDeviceType.Equals(Microsoft.UI.Input.PointerDeviceType.Mouse)
            && IsAutoPlayEnabled && _autoPlayTimer != null)
            _autoPlayTimer.Start();
    }

    #endregion
}
