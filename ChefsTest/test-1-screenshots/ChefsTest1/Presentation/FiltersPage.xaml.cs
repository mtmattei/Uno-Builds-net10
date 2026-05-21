namespace ChefsTest1.Presentation;

public sealed partial class FiltersPage : Page
{
    public FiltersPage()
    {
        this.InitializeComponent();
    }

    public FiltersViewModel? ViewModel => DataContext as FiltersViewModel;
}
