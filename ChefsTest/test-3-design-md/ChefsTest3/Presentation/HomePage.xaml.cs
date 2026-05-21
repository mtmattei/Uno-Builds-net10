using System;
using ChefsTest3.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ChefsTest3.Presentation;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel => (HomeViewModel)DataContext;

    public HomePage()
    {
        this.InitializeComponent();
    }

    private void OnTabSelected(object sender, string tab)
    {
        if (tab == "Search")
        {
            ViewModel.OpenSearchCommand.Execute(null);
        }
        else if (tab == "Favorites")
        {
            ViewModel.OpenFavoritesCommand.Execute(null);
        }
    }

    private void OnSearchFocus(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenSearchCommand.Execute(null);
    }

    private void OnSearchTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.OpenSearchCommand.Execute(null);
    }

    private void OnTrendingRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData recipe)
        {
            ViewModel.OpenRecipeCommand.Execute(recipe);
        }
    }

    private void OnCategoryClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CategoryData cat)
        {
            ViewModel.OpenCategoryCommand.Execute(cat);
        }
    }

    private void OnContributorClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is UserData user)
        {
            ViewModel.OpenContributorCommand.Execute(user);
        }
    }
}
