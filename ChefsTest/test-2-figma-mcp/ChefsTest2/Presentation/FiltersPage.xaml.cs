using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class FiltersPage : Page
{
    public FiltersPage() { this.InitializeComponent(); }
    public FiltersViewModel? ViewModel => DataContext as FiltersViewModel;
}
