namespace ChefsTest1.Presentation;

public sealed partial class CookbookDetailPage : Page
{
    public CookbookDetailPage()
    {
        this.InitializeComponent();
    }

    public CookbookDetailViewModel? ViewModel => DataContext as CookbookDetailViewModel;
}
