namespace ChefsTest5.Presentation;

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand SignUpCommand { get; }

    public RegisterViewModel(INavigator navigator)
    {
        _navigator = navigator;
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        SignUpCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<MainShellViewModel>(this, qualifier: Qualifiers.ClearBackStack));
    }
}
