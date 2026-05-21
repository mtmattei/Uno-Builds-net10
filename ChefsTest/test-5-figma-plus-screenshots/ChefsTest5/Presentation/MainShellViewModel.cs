namespace ChefsTest5.Presentation;

public partial class MainShellViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private int selectedIndex;

    public bool IsHomeSelected => SelectedIndex == 0;
    public bool IsSearchSelected => SelectedIndex == 1;
    public bool IsFavoritesSelected => SelectedIndex == 2;

    partial void OnSelectedIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsHomeSelected));
        OnPropertyChanged(nameof(IsSearchSelected));
        OnPropertyChanged(nameof(IsFavoritesSelected));
    }

    public IRelayCommand<string> SelectTabCommand { get; }

    public MainShellViewModel(INavigator navigator)
    {
        _navigator = navigator;
        SelectTabCommand = new RelayCommand<string>(idx =>
        {
            if (int.TryParse(idx, out var i)) SelectedIndex = i;
        });
    }
}
