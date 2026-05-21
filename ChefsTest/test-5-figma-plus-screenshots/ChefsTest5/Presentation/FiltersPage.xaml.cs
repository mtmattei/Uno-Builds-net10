namespace ChefsTest5.Presentation;

public sealed partial class FiltersPage : Page
{
    public FiltersViewModel ViewModel { get; private set; } = default!;

    public FiltersPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is FiltersViewModel vm) ViewModel = vm; };
    }
}
