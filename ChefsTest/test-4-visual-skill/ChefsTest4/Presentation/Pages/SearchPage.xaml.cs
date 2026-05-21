using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class SearchPage : UserControl
{
    public SearchPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public SearchViewModel? ViewModel => DataContext as SearchViewModel;
}
