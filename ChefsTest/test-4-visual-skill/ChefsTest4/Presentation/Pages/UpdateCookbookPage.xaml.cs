using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class UpdateCookbookPage : UserControl
{
    public UpdateCookbookPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public UpdateCookbookViewModel? ViewModel => DataContext as UpdateCookbookViewModel;
}
