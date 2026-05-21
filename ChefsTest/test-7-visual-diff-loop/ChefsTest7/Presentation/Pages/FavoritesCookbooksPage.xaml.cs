namespace ChefsTest7.Presentation.Pages;

public sealed partial class FavoritesCookbooksPage : Microsoft.UI.Xaml.Controls.Page
{
    public FavoritesCookbooksViewModel? ViewModel => DataContext as FavoritesCookbooksViewModel;
    public FavoritesCookbooksPage() => this.InitializeComponent();
}
