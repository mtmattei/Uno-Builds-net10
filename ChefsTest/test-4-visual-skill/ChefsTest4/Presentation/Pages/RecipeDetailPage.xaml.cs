using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class RecipeDetailPage : UserControl
{
    public RecipeDetailPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public RecipeDetailViewModel? ViewModel => DataContext as RecipeDetailViewModel;
}
