namespace ChefsTest5.Presentation;

public sealed partial class HomeView : UserControl
{
    public HomeViewModel ViewModel { get; }

    public HomeView()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<HomeViewModel>();
        DataContext = ViewModel;
        _ = ViewModel.LoadAsync();
    }
}
