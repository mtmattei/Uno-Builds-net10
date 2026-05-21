using System.Collections.ObjectModel;

namespace ChefsTest6.Services;

public class CookbookService : ICookbookService
{
    private readonly ObservableCollection<CookbookData> _cookbooks;
    private readonly IRecipeService _recipes;

    public CookbookService(IRecipeService recipes)
    {
        _recipes = recipes;
        var raw = DataLoader.Load<List<CookbookSeed>>("Cookbooks.json") ?? new();
        _cookbooks = new ObservableCollection<CookbookData>();
        foreach (var seed in raw)
        {
            var cb = new CookbookData
            {
                Id = seed.Id == Guid.Empty ? Guid.NewGuid() : seed.Id,
                UserId = seed.UserId,
                Name = seed.Name,
            };
            if (seed.Recipes is not null)
            {
                foreach (var r in seed.Recipes)
                {
                    var actual = recipes.FindById(r.Id) ?? r;
                    cb.Recipes.Add(actual);
                }
            }
            _cookbooks.Add(cb);
        }
    }

    public ObservableCollection<CookbookData> All() => _cookbooks;

    public CookbookData? FindById(Guid id) => _cookbooks.FirstOrDefault(c => c.Id == id);

    public CookbookData Create(string name, IEnumerable<RecipeData> recipes)
    {
        var cb = new CookbookData
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty,
            Name = name,
        };
        foreach (var r in recipes) cb.Recipes.Add(r);
        _cookbooks.Add(cb);
        return cb;
    }

    public void Update(CookbookData cookbook, string name, IEnumerable<RecipeData> recipes)
    {
        cookbook.Name = name;
        cookbook.Recipes.Clear();
        foreach (var r in recipes) cookbook.Recipes.Add(r);
    }

    private class CookbookSeed
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public List<RecipeData>? Recipes { get; set; }
    }
}
