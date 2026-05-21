namespace ChefsTest5.Presentation;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; private set; } = default!;

    public ProfilePage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is ProfileViewModel vm) ViewModel = vm; };
    }
}
