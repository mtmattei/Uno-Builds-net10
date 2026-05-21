namespace ChefsTest1.Presentation;

public sealed partial class OtherProfilePage : Page
{
    public OtherProfilePage()
    {
        this.InitializeComponent();
    }

    public OtherProfileViewModel? ViewModel => DataContext as OtherProfileViewModel;
}
