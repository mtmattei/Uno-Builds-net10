namespace ChefsTest6.Services;

public interface IAppThemeService
{
    bool IsDark { get; }
    event EventHandler? ThemeChanged;
    void Toggle();
    void Set(bool isDark);
    void RegisterRoot(FrameworkElement root);
}

public sealed class AppThemeService : IAppThemeService
{
    private readonly List<WeakReference<FrameworkElement>> _roots = new();
    public bool IsDark { get; private set; }
    public event EventHandler? ThemeChanged;

    public void RegisterRoot(FrameworkElement root)
    {
        _roots.Add(new WeakReference<FrameworkElement>(root));
        Apply(root);
    }

    public void Toggle() => Set(!IsDark);

    public void Set(bool isDark)
    {
        IsDark = isDark;
        for (int i = _roots.Count - 1; i >= 0; i--)
        {
            if (_roots[i].TryGetTarget(out var fe))
            {
                Apply(fe);
            }
            else
            {
                _roots.RemoveAt(i);
            }
        }
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Apply(FrameworkElement fe)
    {
        try { fe.RequestedTheme = IsDark ? ElementTheme.Dark : ElementTheme.Light; }
        catch { /* best-effort */ }
    }
}
