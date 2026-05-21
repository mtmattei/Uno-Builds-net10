namespace ChefsTest7.Presentation.Pages;

public sealed partial class FiltersPage : Microsoft.UI.Xaml.Controls.Page
{
    public FiltersViewModel? ViewModel => DataContext as FiltersViewModel;
    public FiltersPage() => this.InitializeComponent();
}
