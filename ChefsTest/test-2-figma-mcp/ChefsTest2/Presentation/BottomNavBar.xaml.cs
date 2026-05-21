using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Navigation;

namespace ChefsTest2.Presentation;

public sealed partial class BottomNavBar : UserControl
{
    public static readonly DependencyProperty ActiveTabProperty =
        DependencyProperty.Register(nameof(ActiveTab), typeof(string), typeof(BottomNavBar),
            new PropertyMetadata("Home", OnActiveTabChanged));

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public static readonly DependencyProperty IsSearchActiveProperty =
        DependencyProperty.Register(nameof(IsSearchActive), typeof(bool), typeof(BottomNavBar),
            new PropertyMetadata(false));
    public bool IsSearchActive
    {
        get => (bool)GetValue(IsSearchActiveProperty);
        set => SetValue(IsSearchActiveProperty, value);
    }

    public static readonly DependencyProperty IsFavoritesActiveProperty =
        DependencyProperty.Register(nameof(IsFavoritesActive), typeof(bool), typeof(BottomNavBar),
            new PropertyMetadata(false));
    public bool IsFavoritesActive
    {
        get => (bool)GetValue(IsFavoritesActiveProperty);
        set => SetValue(IsFavoritesActiveProperty, value);
    }

    public BottomNavBar()
    {
        this.InitializeComponent();
        UpdateActiveStates();
    }

    private static void OnActiveTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BottomNavBar nav) nav.UpdateActiveStates();
    }

    private void UpdateActiveStates()
    {
        IsSearchActive = string.Equals(ActiveTab, "Search", StringComparison.OrdinalIgnoreCase);
        IsFavoritesActive = string.Equals(ActiveTab, "Favorites", StringComparison.OrdinalIgnoreCase);
    }

    private async void HomeButton_Click(object sender, RoutedEventArgs e)
        => await NavigateAsync("Home");

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
        => await NavigateAsync("Search");

    private async void FavoritesButton_Click(object sender, RoutedEventArgs e)
        => await NavigateAsync("Favorites");

    private async Task NavigateAsync(string route)
    {
        if (string.Equals(ActiveTab, route, StringComparison.OrdinalIgnoreCase)) return;
        var nav = this.Navigator();
        if (nav is null) return;
        await nav.NavigateRouteAsync(this, $"-/{route}");
    }
}
