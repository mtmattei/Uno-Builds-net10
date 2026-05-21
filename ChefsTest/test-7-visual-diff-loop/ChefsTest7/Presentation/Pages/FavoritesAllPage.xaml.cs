namespace ChefsTest7.Presentation.Pages;

public sealed partial class FavoritesAllPage : Microsoft.UI.Xaml.Controls.Page
{
    public FavoritesAllViewModel? ViewModel => DataContext as FavoritesAllViewModel;
    public FavoritesAllPage() => this.InitializeComponent();
}
