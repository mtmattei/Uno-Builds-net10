using System;
using System.Collections.Generic;

namespace ChefsTest1.Models;

public class Recipe
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public int Serves { get; set; }
    public TimeSpanLike CookTime { get; set; } = new();
    public int Difficulty { get; set; }
    public List<Ingredient>? Ingredients { get; set; }
    public string? Calories { get; set; }
    public List<Review>? Reviews { get; set; }
    public List<Step>? Steps { get; set; }
    public string? Details { get; set; }
    public Category? Category { get; set; }
    public User? Creator { get; set; }
    public DateTime Date { get; set; }
    public bool Save { get; set; }
    public Nutrition? Nutrition { get; set; }
    public bool IsFavorite { get; set; }

    public string? CategoryName => Category?.Name;
    public string CookTimeMinutes => $"{(int)CookTime.AsTimeSpan.TotalMinutes} mins";
    public string DifficultyLabel => Difficulty switch
    {
        0 => "Easy",
        1 => "Easy",
        2 => "Intermediate",
        _ => "Advanced",
    };
}

public class TimeSpanLike
{
    public long Ticks { get; set; }
    public TimeSpan AsTimeSpan => TimeSpan.FromTicks(Ticks);
}

public class Ingredient
{
    public string? UrlIcon { get; set; }
    public string? Name { get; set; }
    public string? Quantity { get; set; }
}

public class Step
{
    public string? UrlVideo { get; set; }
    public string? Name { get; set; }
    public int Number { get; set; }
    public TimeSpanLike CookTime { get; set; } = new();
    public List<string>? Cookware { get; set; }
    public List<string>? Ingredients { get; set; }
    public string? Description { get; set; }

    public string DisplayTitle => $"{Number}- {Name}";
    public string ShortDescription
    {
        get
        {
            if (string.IsNullOrEmpty(Description)) return string.Empty;
            var firstLine = Description.Split('\n')[0];
            return firstLine.Length > 140 ? firstLine.Substring(0, 137) + "..." : firstLine;
        }
    }
}

public class Review
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public string? UrlAuthorImage { get; set; }
    public Guid CreatedBy { get; set; }
    public string? PublisherName { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public List<Guid>? Likes { get; set; }
    public List<Guid>? Dislikes { get; set; }

    public int LikeCount => Likes?.Count ?? 0;
    public int DislikeCount => Dislikes?.Count ?? 0;
}

public class Nutrition
{
    public double Protein { get; set; }
    public double ProteinBase { get; set; }
    public double Carbs { get; set; }
    public double CarbsBase { get; set; }
    public double Fat { get; set; }
    public double FatBase { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string? UrlIcon { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }

    public string RecipesLabel { get; set; } = "0 recipes";
}

public class User
{
    public Guid Id { get; set; }
    public string? UrlProfileImage { get; set; }
    public string? FullName { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public long? Followers { get; set; }
    public long? Following { get; set; }
    public long? Recipes { get; set; }
    public bool IsCurrent { get; set; }

    public string ShortName => string.IsNullOrEmpty(FullName) ? string.Empty : FullName.Replace(" ", "").Replace(".", "");
    public string FollowersText => Followers.HasValue ? $"{Followers} recipes" : "0 recipes";
}

public class Cookbook
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public List<Recipe>? Recipes { get; set; }

    public int RecipeCount => Recipes?.Count ?? 0;
    public string RecipeCountLabel => $"{RecipeCount} recipes";
    public string? Image1 => Recipes != null && Recipes.Count > 0 ? Recipes[0].ImageUrl : null;
    public string? Image2 => Recipes != null && Recipes.Count > 1 ? Recipes[1].ImageUrl : null;
    public string? Image3 => Recipes != null && Recipes.Count > 2 ? Recipes[2].ImageUrl : null;
    public string? Image4 => Recipes != null && Recipes.Count > 3 ? Recipes[3].ImageUrl : null;
}

public class Notification
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsRead { get; set; }
    public DateTime Date { get; set; }
}
