using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class NearMeViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;

    public ObservableCollection<User> NearbyChefs { get; } = new();
    [ObservableProperty] private User? selectedChef;

    public NearMeViewModel(INavigator navigator, IUserService users)
    {
        _navigator = navigator;
        _users = users;
        BackCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        OpenChefCommand = new AsyncRelayCommand<User>(u => _navigator.NavigateRouteAsync(this, "OtherProfile", data: u!)!);
        _ = LoadAsync();
    }

    public ICommand BackCommand { get; }
    public ICommand OpenChefCommand { get; }

    private async Task LoadAsync()
    {
        var creators = await _users.GetPopularCreatorsAsync();
        foreach (var u in creators.Take(3)) NearbyChefs.Add(u);
        SelectedChef = NearbyChefs.FirstOrDefault();
    }
}
