namespace ChefsTest6.Services;

public class UserService : IUserService
{
    private readonly List<UserData> _users;
    private UserData _current;

    public UserService()
    {
        _users = DataLoader.Load<List<UserData>>("Users.json") ?? new();
        _current = _users.FirstOrDefault() ?? new UserData
        {
            Id = Guid.NewGuid(),
            FullName = "Guest",
            Email = "guest@example.com",
            Followers = 0,
            Following = 0,
            Recipes = 0,
        };
        _current.IsCurrent = true;
    }

    public UserData Current => _current;

    public IReadOnlyList<UserData> All() => _users;

    public IReadOnlyList<UserData> PopularContributors() =>
        _users.OrderByDescending(u => u.Followers ?? 0).Take(8).ToList();

    public UserData? FindById(Guid id) => _users.FirstOrDefault(u => u.Id == id);

    public bool Authenticate(string email, string password, out UserData? user)
    {
        user = _users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)
            && u.Password == password);
        if (user is null)
        {
            // demo-mode bypass: any non-empty username/password lands as the seed user
            user = !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password) ? _current : null;
        }
        return user is not null;
    }

    public void UpdateCurrent(string fullName, string email, string phone)
    {
        _current.FullName = fullName;
        _current.Email = email;
        _current.PhoneNumber = phone;
    }
}
