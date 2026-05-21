namespace ChefsTest5.Presentation;

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    public IAsyncRelayCommand CloseCommand { get; }
    public IRelayCommand ResetCommand { get; }
    public IAsyncRelayCommand ApplyCommand { get; }

    public FiltersViewModel(INavigator navigator)
    {
        _navigator = navigator;
        CloseCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        ResetCommand = new RelayCommand(() => { });
        ApplyCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
    }
}

public partial class NearMeMapViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    public IAsyncRelayCommand BackCommand { get; }
    public NearMeMapViewModel(INavigator navigator)
    {
        _navigator = navigator;
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
    }
}
