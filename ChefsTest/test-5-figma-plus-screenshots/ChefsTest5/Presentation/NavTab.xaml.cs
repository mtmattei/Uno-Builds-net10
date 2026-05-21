using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ChefsTest5.Presentation;

public sealed partial class NavTab : UserControl
{
    public NavTab()
    {
        this.InitializeComponent();
        UpdateVisuals();
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph), typeof(string), typeof(NavTab),
        new PropertyMetadata("", (d, _) => ((NavTab)d).UpdateVisuals()));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(NavTab),
        new PropertyMetadata("", (d, _) => ((NavTab)d).UpdateVisuals()));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive), typeof(bool), typeof(NavTab),
        new PropertyMetadata(false, (d, _) => ((NavTab)d).UpdateVisuals()));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(NavTab), new PropertyMetadata(null,
        (d, e) => ((NavTab)d).RootButton.Command = (ICommand?)e.NewValue));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(NavTab), new PropertyMetadata(null,
        (d, e) => ((NavTab)d).RootButton.CommandParameter = e.NewValue));

    private void UpdateVisuals()
    {
        if (GlyphText is not null) GlyphText.Text = Glyph;
        if (LabelText is not null) LabelText.Text = Label;
        if (IconHost is not null)
        {
            var key = IsActive ? "ChefCreamBrush" : null;
            IconHost.Background = key is not null && Application.Current.Resources.TryGetValue(key, out var b)
                ? (Brush)b!
                : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
    }
}
