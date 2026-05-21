using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class FiltersPage : UserControl
{
    public FiltersPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public FiltersViewModel? ViewModel => DataContext as FiltersViewModel;
}
