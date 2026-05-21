using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class FiltersPage : Page
{
    public FiltersViewModel ViewModel => (FiltersViewModel)DataContext;

    public FiltersPage()
    {
        this.InitializeComponent();
    }
}
