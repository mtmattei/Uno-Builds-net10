namespace ChefsTest1.Presentation;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool rememberMe;
    [ObservableProperty] private string? errorMessage;

    public LoginViewModel(INavigator navigator, IUserService users)
    {
        _navigator = navigator;
        _users = users;
        LoginCommand = new AsyncRelayCommand(DoLoginAsync);
        AppleLoginCommand = new AsyncRelayCommand(DoLoginAsync);
        GoogleLoginCommand = new AsyncRelayCommand(DoLoginAsync);
        ForgotCommand = new AsyncRelayCommand(DoLoginAsync);
        RegisterCommand = new AsyncRelayCommand(GoRegisterAsync);
    }

    public ICommand LoginCommand { get; }
    public ICommand AppleLoginCommand { get; }
    public ICommand GoogleLoginCommand { get; }
    public ICommand ForgotCommand { get; }
    public ICommand RegisterCommand { get; }

    private async Task DoLoginAsync()
    {
        ErrorMessage = null;
        await _navigator.NavigateRouteAsync(this, "-/Main");
    }

    private async Task GoRegisterAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Register");
    }
}
