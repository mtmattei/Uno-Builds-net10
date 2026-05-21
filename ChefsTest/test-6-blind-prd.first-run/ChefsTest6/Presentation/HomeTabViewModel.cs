using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class HomeTabViewModel : ObservableObject
{
    private readonly MainViewModel _parent;
    private readonly IChefService _chef;

    public ObservableCollection<Recipe> Trending { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Recipe> RecentlyAdded { get; } = new();
    public ObservableCollection<User> PopularContributors { get; } = new();

    public string GreetingName => _chef.CurrentUser?.FullName?.Split(' ').FirstOrDefault() ?? "there";
    public string GreetingMessage => "What are you cooking today?";

    public HomeTabViewModel(MainViewModel parent, IChefService chef)
    {
        _parent = parent;
        _chef = chef;
    }

    public void Populate()
    {
        Trending.Clear();
        foreach (var r in _chef.Trending) Trending.Add(r);

        Categories.Clear();
        foreach (var c in _chef.Categories) Categories.Add(c);

        RecentlyAdded.Clear();
        foreach (var r in _chef.RecentlyAdded) RecentlyAdded.Add(r);

        PopularContributors.Clear();
        foreach (var u in _chef.PopularContributors) PopularContributors.Add(u);

        OnPropertyChanged(nameof(GreetingName));
    }

    public IAsyncRelayCommand<Recipe?> OpenRecipeCommand => _parent.OpenRecipeCommand;
    public IAsyncRelayCommand<User?> OpenContributorCommand => _parent.OpenOtherProfileCommand;
    public IRelayCommand<Category?> OpenCategoryCommand => _parent.OpenCategoryCommand;
    public IRelayCommand<Recipe?> ToggleFavoriteCommand => _parent.ToggleFavoriteCommand;
    public IAsyncRelayCommand OpenNotificationsCommand => _parent.OpenNotificationsCommand;
    public IRelayCommand GoToProfileCommand => _parent.GoToProfileCommand;
    public IAsyncRelayCommand OpenMapCommand => _parent.OpenMapCommand;
    public IRelayCommand GoToSearchCommand => _parent.GoToSearchCommand;
}
