namespace ChefsTest6.Presentation;

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    [ObservableProperty]
    private string? username;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private string? password;

    [ObservableProperty]
    private string? errorMessage;

    public RegisterViewModel(INavigator navigator, IChefService chef)
    {
        _navigator = navigator;
        _chef = chef;
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) ||
            !Email!.Contains('@') ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required and email must be valid.";
            return;
        }

        var user = await _chef.AuthenticateAsync(new LoginRequest(Email, Password));
        // For demo we just bounce into Main using whatever fixture lookup returns; no mutation of users.
        await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await _navigator.NavigateViewModelAsync<LoginViewModel>(this);
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await _navigator.NavigateViewModelAsync<LoginViewModel>(this);
    }
}
