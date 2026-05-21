namespace ChefsTest5.Presentation;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; private set; } = default!;

    public SettingsPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is SettingsViewModel vm) ViewModel = vm; };
    }
}
