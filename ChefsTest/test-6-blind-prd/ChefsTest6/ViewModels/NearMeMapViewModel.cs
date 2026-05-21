namespace ChefsTest6.ViewModels;

public partial class NearMeMapViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;

    public IReadOnlyList<UserData> Chefs { get; }

    [ObservableProperty]
    private UserData? selectedChef;

    public NearMeMapViewModel(INavigator navigator, IUserService users)
    {
        _navigator = navigator;
        _users = users;
        Chefs = users.PopularContributors();
        SelectedChef = Chefs.FirstOrDefault();
    }

    [RelayCommand]
    private void SelectChef(UserData? user) => SelectedChef = user;

    [RelayCommand]
    private async Task OpenSelectedProfile()
    {
        if (SelectedChef is null) return;
        await _navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: SelectedChef);
    }
}
