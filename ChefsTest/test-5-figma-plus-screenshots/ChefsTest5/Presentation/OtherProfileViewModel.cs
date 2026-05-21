namespace ChefsTest5.Presentation;

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private UserData user = new();
    public IAsyncRelayCommand BackCommand { get; }

    public OtherProfileViewModel(INavigator navigator, UserData? data = null)
    {
        _navigator = navigator;
        if (data is not null) User = data;
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
    }
}
