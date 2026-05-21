using Microsoft.UI.Xaml;

namespace ChefsTest7.Services;

public interface IThemeToggleService
{
    bool IsDark { get; set; }
    void Toggle();
    void Attach(FrameworkElement root);
}

public class ThemeToggleService : IThemeToggleService
{
    private FrameworkElement? _root;

    public void Attach(FrameworkElement root) => _root = root;

    public bool IsDark
    {
        get => _root?.ActualTheme == ElementTheme.Dark;
        set
        {
            if (_root is not null)
                _root.RequestedTheme = value ? ElementTheme.Dark : ElementTheme.Light;
        }
    }

    public void Toggle() => IsDark = !IsDark;
}
