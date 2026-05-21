namespace ChefsTest6.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    public RegisterViewModel(INavigator navigator, IUserService users)
    {
        _navigator = navigator;
        _users = users;
    }

    [RelayCommand]
    private async Task SignUp()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required.";
            return;
        }
        if (!Email.Contains('@') || !Email.Contains('.'))
        {
            ErrorMessage = "Email looks invalid.";
            return;
        }

        await _navigator.NavigateViewModelAsync<HomeViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async Task GoToLogin()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
