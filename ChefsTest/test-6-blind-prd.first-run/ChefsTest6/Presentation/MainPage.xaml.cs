namespace ChefsTest6.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Home.Populate();
            vm.Search.Populate();
            vm.Favorites.Populate();
            vm.Profile.Populate();
        }
    }
}
