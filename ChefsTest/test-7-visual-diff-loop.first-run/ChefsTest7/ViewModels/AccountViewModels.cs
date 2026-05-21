using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest7.Models;
using ChefsTest7.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    [ObservableProperty] private UserData? user;
    public ObservableCollection<RecipeData> MyRecipes { get; } = new();
    public bool HasRecipes => MyRecipes.Count > 0;

    public ProfileViewModel(INavigator navigator, IChefsDataService data)
    { _navigator = navigator; _data = data; _ = LoadAsync(); }

    private async Task LoadAsync()
    {
        User = await _data.GetCurrentUserAsync();
        var recipes = await _data.GetRecipesAsync();
        var mine = recipes.Where(r => User is not null && r.UserId == User.Id).ToList();
        foreach (var r in mine) MyRecipes.Add(r);
        OnPropertyChanged(nameof(HasRecipes));
    }

    [RelayCommand] private async Task OpenSettings() => await _navigator.NavigateRouteAsync(this, "Settings");
    [RelayCommand] private async Task OpenNotifications() => await _navigator.NavigateRouteAsync(this, "Notifications");
    [RelayCommand] private async Task Back() => await _navigator.NavigateBackAsync(this);
    [RelayCommand] private async Task OpenRecipe(RecipeData? r) { if (r is not null) await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r); }
    [RelayCommand] private async Task AddRecipe() => await _navigator.NavigateRouteAsync(this, "Search");
}

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    [ObservableProperty] private UserData? user;
    public ObservableCollection<RecipeData> Recipes { get; } = new();

    public OtherProfileViewModel(INavigator navigator, IChefsDataService data, UserData? user = null)
    {
        _navigator = navigator; _data = data; User = user;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (User is null)
        {
            var users = await _data.GetUsersAsync();
            User = users.Skip(1).FirstOrDefault();
        }
        var recipes = await _data.GetRecipesAsync();
        var theirs = recipes.Where(r => User is not null && r.UserId == User.Id).ToList();
        foreach (var r in theirs) Recipes.Add(r);
    }

    [RelayCommand] private async Task Back() => await _navigator.NavigateBackAsync(this);
    [RelayCommand] private async Task OpenRecipe(RecipeData? r) { if (r is not null) await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r); }
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private readonly IThemeToggleService _theme;

    [ObservableProperty] private string fullName = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string phoneNumber = "";
    [ObservableProperty] private bool notificationsOn = true;
    [ObservableProperty] private bool nightMode;
    [ObservableProperty] private string? saveStatus;

    public SettingsViewModel(INavigator navigator, IChefsDataService data, IThemeToggleService theme)
    {
        _navigator = navigator; _data = data; _theme = theme;
        NightMode = _theme.IsDark;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var me = await _data.GetCurrentUserAsync();
        if (me is null) return;
        FullName = me.FullName ?? string.Empty;
        Email = me.Email ?? string.Empty;
        PhoneNumber = me.PhoneNumber ?? string.Empty;
    }

    partial void OnNightModeChanged(bool value) { _theme.IsDark = value; }

    [RelayCommand]
    private async Task SaveChanges()
    {
        var me = await _data.GetCurrentUserAsync();
        if (me is null) return;
        me.FullName = FullName; me.Email = Email; me.PhoneNumber = PhoneNumber;
        await _data.UpdateCurrentUserAsync(me);
        SaveStatus = "Saved.";
    }

    [RelayCommand] private async Task Back() => await _navigator.NavigateBackAsync(this);
    [RelayCommand] private async Task LogOut() => await _navigator.NavigateRouteAsync(this, "Login");
}

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    public ObservableCollection<NotificationData> All { get; } = new();
    public ObservableCollection<NotificationData> Unread { get; } = new();
    public ObservableCollection<NotificationData> Read { get; } = new();

    [ObservableProperty] private int selectedTab; // 0:All 1:Unread 2:Read
    public bool HasItems => All.Count > 0;

    public NotificationsViewModel(INavigator navigator, IChefsDataService data)
    { _navigator = navigator; _data = data; _ = LoadAsync(); }

    private async Task LoadAsync()
    {
        var items = await _data.GetNotificationsAsync();
        foreach (var n in items) All.Add(n);
        foreach (var n in items.Where(x => !x.IsRead)) Unread.Add(n);
        foreach (var n in items.Where(x => x.IsRead)) Read.Add(n);
        OnPropertyChanged(nameof(HasItems));
    }

    [RelayCommand] private async Task Close() => await _navigator.NavigateBackAsync(this);
}

public partial class NearMeMapViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    public ObservableCollection<UserData> Chefs { get; } = new();
    [ObservableProperty] private UserData? selectedChef;

    public NearMeMapViewModel(INavigator navigator, IChefsDataService data)
    { _navigator = navigator; _data = data; _ = LoadAsync(); }

    private async Task LoadAsync()
    {
        var items = await _data.GetPopularCreatorsAsync();
        foreach (var u in items) Chefs.Add(u);
        SelectedChef = Chefs.FirstOrDefault();
    }

    [RelayCommand] private async Task Back() => await _navigator.NavigateBackAsync(this);
    [RelayCommand] private async Task OpenChef() { if (SelectedChef is not null) await _navigator.NavigateRouteAsync(this, "OtherProfile", data: SelectedChef); }
    [RelayCommand] private void SelectChef(UserData? u) { if (u is not null) SelectedChef = u; }
}

