namespace ChefsTest6.ViewModels;

public class MainViewModel
{
    private readonly INavigator _navigator;
    public MainViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }
}
