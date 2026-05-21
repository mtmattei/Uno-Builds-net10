using System.Collections.ObjectModel;

namespace ChefsTest6.Services;

public interface IRecipeService
{
    IReadOnlyList<RecipeData> All();
    IReadOnlyList<RecipeData> Trending();
    IReadOnlyList<RecipeData> Popular();
    IReadOnlyList<RecipeData> RecentlyAdded();
    IReadOnlyList<RecipeData> Search(string? text, CategoryData? category, int? maxMinutes, int? difficulty);
    IReadOnlyList<RecipeData> Favorites();
    void ToggleFavorite(RecipeData recipe);
    IReadOnlyList<CategoryData> Categories();
    RecipeData? FindById(Guid id);
}

public interface ICookbookService
{
    ObservableCollection<CookbookData> All();
    CookbookData? FindById(Guid id);
    CookbookData Create(string name, IEnumerable<RecipeData> recipes);
    void Update(CookbookData cookbook, string name, IEnumerable<RecipeData> recipes);
}

public interface IUserService
{
    UserData Current { get; }
    IReadOnlyList<UserData> All();
    IReadOnlyList<UserData> PopularContributors();
    UserData? FindById(Guid id);
    bool Authenticate(string email, string password, out UserData? user);
    void UpdateCurrent(string fullName, string email, string phone);
}

public interface INotificationService
{
    ObservableCollection<NotificationData> All();
    void MarkRead(NotificationData notification);
}

public interface IAppSettingsService
{
    bool NotificationsEnabled { get; set; }
    bool NightMode { get; set; }
    event EventHandler? NightModeChanged;
}
