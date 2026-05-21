using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class MapViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public ObservableCollection<User> NearbyChefs { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private User? selectedChef;

    public bool HasSelection => SelectedChef is not null;
    public string Title => "Nearby chefs";
    public string LocationStatus => "Showing 6 chefs near your location";

    public MapViewModel(INavigator navigator, IChefService chef)
    {
        _navigator = navigator;
        _chef = chef;
        foreach (var u in chef.PopularContributors.Take(6)) NearbyChefs.Add(u);
        SelectedChef = NearbyChefs.FirstOrDefault();
    }

    [RelayCommand]
    private void Select(User? user) => SelectedChef = user;

    [RelayCommand]
    private async Task OpenChefAsync(User? user)
    {
        if (user is null) return;
        await _navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: user);
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
