namespace ChefsTest6.Presentation;

public class ShellViewModel
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public ShellViewModel(INavigator navigator, IChefService chef)
    {
        _navigator = navigator;
        _chef = chef;
        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        await _chef.InitializeAsync();
        await _navigator.NavigateViewModelAsync<OnboardingViewModel>(this);
    }
}
