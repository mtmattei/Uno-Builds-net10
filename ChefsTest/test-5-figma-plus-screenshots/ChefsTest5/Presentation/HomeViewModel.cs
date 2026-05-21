using System.Collections.ObjectModel;
using System.Linq;

namespace ChefsTest5.Presentation;

public partial class HomeViewModel : ObservableObject
{
    private readonly IChefsDataService _data;
    private readonly INavigator _navigator;

    public ObservableCollection<RecipeData> Trending { get; } = new();
    public ObservableCollection<RecipeData> Recent { get; } = new();
    public ObservableCollection<CategoryData> Categories { get; } = new();
    public ObservableCollection<UserData> Contributors { get; } = new();

    public IAsyncRelayCommand GoProfileCommand { get; }
    public IAsyncRelayCommand GoNotificationsCommand { get; }
    public IAsyncRelayCommand GoNearMeCommand { get; }
    public IAsyncRelayCommand<RecipeData> OpenRecipeCommand { get; }

    public HomeViewModel(IChefsDataService data, INavigator navigator)
    {
        _data = data;
        _navigator = navigator;
        GoProfileCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<ProfileViewModel>(this));
        GoNotificationsCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<NotificationsViewModel>(this));
        GoNearMeCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<NearMeMapViewModel>(this));
        OpenRecipeCommand = new AsyncRelayCommand<RecipeData>(async recipe =>
        {
            if (recipe is null) return;
            await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
        });
    }

    public async Task LoadAsync()
    {
        if (Trending.Count > 0) return;
        var trending = await _data.GetTrendingAsync();
        var recent = await _data.GetRecentAsync();
        var cats = await _data.GetCategoriesAsync();
        var contrib = await _data.GetPopularCreatorsAsync();
        foreach (var r in trending) Trending.Add(r);
        foreach (var r in recent) Recent.Add(r);
        foreach (var c in cats) Categories.Add(c);
        foreach (var u in contrib) Contributors.Add(u);
    }
}
