using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChefsTest7.Models;

public partial record Category(int Id, string UrlIcon, string Name);

public partial record Ingredient(string UrlIcon, string Name, string Quantity);

public partial record TickDuration(long ticks);

public partial record RecipeStep(
    string Name,
    TickDuration CookTime,
    IList<string> Cookware,
    string Description,
    IList<string> Ingredients,
    int Number,
    string UrlVideo);

public partial record Review(string UserId, int Rating, string Comment, DateTime Date);

public partial record Recipe(
    string Name,
    string UserId,
    string Id,
    IList<RecipeStep> Steps,
    string ImageUrl,
    int Serves,
    TickDuration CookTime,
    int Difficulty,
    IList<Ingredient> Ingredients,
    IList<Review>? Reviews,
    int Category);

public partial record User(
    string Id,
    string UrlProfileImage,
    string FullName,
    string Description,
    string Email,
    string PhoneNumber,
    string Password,
    int Followers,
    int Following,
    int Recipes);

public partial record Cookbook(
    string Id,
    string Name,
    string UserId,
    int PinsNumber,
    IList<Recipe>? Recipes);

public partial record Notification(string Title, string Description, bool IsRead, DateTime Date);

public partial record SavedRecipe(string RecipeId, string UserId);

public partial record SavedCookbook(string CookbookId, string UserId);
