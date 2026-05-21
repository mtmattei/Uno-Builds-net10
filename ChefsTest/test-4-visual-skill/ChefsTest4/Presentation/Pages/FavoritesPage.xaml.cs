using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class FavoritesPage : UserControl
{
    public FavoritesPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public FavoritesViewModel? ViewModel => DataContext as FavoritesViewModel;
}
