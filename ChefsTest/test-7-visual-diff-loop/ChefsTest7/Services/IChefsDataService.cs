using System.Collections.Generic;
using System.Threading.Tasks;
using ChefsTest7.Models;

namespace ChefsTest7.Services;

public interface IChefsDataService
{
    Task InitializeAsync();
    IReadOnlyList<Recipe> Recipes { get; }
    IReadOnlyList<Category> Categories { get; }
    IReadOnlyList<User> Users { get; }
    IReadOnlyList<Cookbook> Cookbooks { get; }
    IReadOnlyList<Notification> Notifications { get; }
    HashSet<string> SavedRecipeIds { get; }
    HashSet<string> SavedCookbookIds { get; }

    User? CurrentUser { get; }
    void SetCurrentUser(User user);

    bool ToggleSavedRecipe(string recipeId);
    bool ToggleSavedCookbook(string cookbookId);

    void AddCookbook(Cookbook cookbook);
    void UpdateCookbook(Cookbook cookbook);
}
