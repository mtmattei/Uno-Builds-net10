namespace ChefsTest1.Presentation;

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    public RegisterViewModel(INavigator navigator)
    {
        _navigator = navigator;
        SignUpCommand = new AsyncRelayCommand(DoSignUpAsync);
        LoginCommand = new AsyncRelayCommand(GoLoginAsync);
    }

    public ICommand SignUpCommand { get; }
    public ICommand LoginCommand { get; }

    private async Task DoSignUpAsync()
    {
        await _navigator.NavigateRouteAsync(this, "-/Main");
    }

    private async Task GoLoginAsync()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
