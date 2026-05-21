namespace ChefsTest1.Presentation;

public sealed partial class UpdateCookbookPage : Page
{
    public UpdateCookbookPage()
    {
        this.InitializeComponent();
    }

    public UpdateCookbookViewModel? ViewModel => DataContext as UpdateCookbookViewModel;
}
