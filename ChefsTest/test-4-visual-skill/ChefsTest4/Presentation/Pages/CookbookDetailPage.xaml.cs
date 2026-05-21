using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class CookbookDetailPage : UserControl
{
    public CookbookDetailPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public CookbookDetailViewModel? ViewModel => DataContext as CookbookDetailViewModel;
}
