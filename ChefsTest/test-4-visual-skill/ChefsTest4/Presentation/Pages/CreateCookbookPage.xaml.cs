using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class CreateCookbookPage : UserControl
{
    public CreateCookbookPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public CreateCookbookViewModel? ViewModel => DataContext as CreateCookbookViewModel;
}
