namespace ChefsTest1.Presentation;

public partial class LiveCookingFinishViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public Recipe Recipe { get; }
    public string AppBarTitle => $"Making {Recipe?.Name}";

    [ObservableProperty] private int rating;

    public LiveCookingFinishViewModel(INavigator navigator, Recipe recipe)
    {
        _navigator = navigator;
        Recipe = recipe;
        BackCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        PreviousCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        FavoriteCommand = new AsyncRelayCommand(() => _navigator.NavigateRouteAsync(this, "-/Main"));
        Rate1Command = new RelayCommand(() => Rating = 1);
        Rate2Command = new RelayCommand(() => Rating = 2);
        Rate3Command = new RelayCommand(() => Rating = 3);
        Rate4Command = new RelayCommand(() => Rating = 4);
        Rate5Command = new RelayCommand(() => Rating = 5);
    }

    public ICommand BackCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand FavoriteCommand { get; }
    public ICommand Rate1Command { get; }
    public ICommand Rate2Command { get; }
    public ICommand Rate3Command { get; }
    public ICommand Rate4Command { get; }
    public ICommand Rate5Command { get; }

    public bool Star1 => Rating >= 1;
    public bool Star2 => Rating >= 2;
    public bool Star3 => Rating >= 3;
    public bool Star4 => Rating >= 4;
    public bool Star5 => Rating >= 5;

    partial void OnRatingChanged(int value)
    {
        OnPropertyChanged(nameof(Star1));
        OnPropertyChanged(nameof(Star2));
        OnPropertyChanged(nameof(Star3));
        OnPropertyChanged(nameof(Star4));
        OnPropertyChanged(nameof(Star5));
    }
}
