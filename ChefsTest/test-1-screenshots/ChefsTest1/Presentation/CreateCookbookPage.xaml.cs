namespace ChefsTest1.Presentation;

public sealed partial class CreateCookbookPage : Page
{
    public CreateCookbookPage()
    {
        this.InitializeComponent();
    }

    public CreateCookbookViewModel? ViewModel => DataContext as CreateCookbookViewModel;
}
