using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ChefsTest3.Presentation;

public sealed partial class ChipButton : UserControl
{
    public ChipButton()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => Apply();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(ChipButton),
            new PropertyMetadata(string.Empty, (d, _) => ((ChipButton)d).Apply()));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ChipButton),
            new PropertyMetadata(false, (d, _) => ((ChipButton)d).Apply()));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    private void Apply()
    {
        if (LabelText is null || RootButton is null) return;
        LabelText.Text = Label ?? string.Empty;
        if (IsSelected)
        {
            RootButton.Background = (Brush)Application.Current.Resources["PrimaryBrush"];
            RootButton.Foreground = (Brush)Application.Current.Resources["TextOnPrimaryBrush"];
            RootButton.BorderBrush = (Brush)Application.Current.Resources["PrimaryBrush"];
        }
        else
        {
            RootButton.Background = (Brush)Application.Current.Resources["ChipUnselectedBrush"];
            RootButton.Foreground = (Brush)Application.Current.Resources["ChipUnselectedTextBrush"];
            RootButton.BorderBrush = (Brush)Application.Current.Resources["DividerBrush"];
        }
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        IsSelected = !IsSelected;
    }
}
