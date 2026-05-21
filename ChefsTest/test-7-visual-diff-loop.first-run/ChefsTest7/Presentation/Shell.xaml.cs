using ChefsTest7.Services;

namespace ChefsTest7.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    public ContentControl ContentControl => Splash;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (App.Services?.GetService(typeof(IThemeToggleService)) is IThemeToggleService theme)
        {
            theme.Attach(this);
        }
    }
}
