using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ChefsTest3.Presentation;

public sealed partial class BottomNavBar : UserControl
{
    public BottomNavBar()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => ApplyActive();
    }

    public static readonly DependencyProperty ActiveTabProperty =
        DependencyProperty.Register(nameof(ActiveTab), typeof(string), typeof(BottomNavBar),
            new PropertyMetadata("Home", (d, _) => ((BottomNavBar)d).ApplyActive()));

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public event EventHandler<string>? TabSelected;

    private void OnHomeClick(object sender, RoutedEventArgs e) => TabSelected?.Invoke(this, "Home");
    private void OnSearchClick(object sender, RoutedEventArgs e) => TabSelected?.Invoke(this, "Search");
    private void OnFavoritesClick(object sender, RoutedEventArgs e) => TabSelected?.Invoke(this, "Favorites");

    private void ApplyActive()
    {
        if (HomeIcon is null) return;
        Brush active = (Brush)Application.Current.Resources["PrimaryBrush"];
        Brush muted = (Brush)Application.Current.Resources["TextSecondaryBrush"];

        HomeIcon.Foreground = ActiveTab == "Home" ? active : muted;
        HomeLabel.Foreground = ActiveTab == "Home" ? active : muted;
        SearchIcon.Foreground = ActiveTab == "Search" ? active : muted;
        SearchLabel.Foreground = ActiveTab == "Search" ? active : muted;
        FavIcon.Foreground = ActiveTab == "Favorites" ? active : muted;
        FavLabel.Foreground = ActiveTab == "Favorites" ? active : muted;
    }
}
