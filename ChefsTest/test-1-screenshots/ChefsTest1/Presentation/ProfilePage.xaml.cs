namespace ChefsTest1.Presentation;

public sealed partial class ProfilePage : Page
{
    public ProfilePage()
    {
        this.InitializeComponent();
    }

    public ProfileViewModel? ViewModel => DataContext as ProfileViewModel;
}
