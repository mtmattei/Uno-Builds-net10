namespace ChefsTest5.Presentation;

public sealed partial class SearchView : UserControl
{
    public SearchViewModel ViewModel { get; }

    public SearchView()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<SearchViewModel>();
        DataContext = ViewModel;
        _ = ViewModel.LoadAsync();
    }
}
