namespace ChefsTest5.Presentation;

public sealed partial class FavoritesView : UserControl
{
    public FavoritesViewModel ViewModel { get; }

    public FavoritesView()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<FavoritesViewModel>();
        DataContext = ViewModel;
        _ = ViewModel.LoadAsync();
    }
}
