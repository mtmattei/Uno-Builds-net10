namespace ChefsTest6.Models;

public enum FilterCategory { None, Popular, Trending, Recent }
public enum FilterCookingTime { None, Under15, Under30, Under60 }
public enum FilterSkill { None, Beginner, Intermediate, Advanced }

public record SearchFilters(
    FilterCategory Category = FilterCategory.None,
    FilterCookingTime CookingTime = FilterCookingTime.None,
    FilterSkill Skill = FilterSkill.None)
{
    public bool IsActive =>
        Category != FilterCategory.None ||
        CookingTime != FilterCookingTime.None ||
        Skill != FilterSkill.None;

    public static SearchFilters Empty { get; } = new();
}
