namespace ChefsTest1.Presentation;

public sealed partial class LiveCookingPage : Page
{
    public LiveCookingPage()
    {
        this.InitializeComponent();
    }

    public LiveCookingViewModel? ViewModel => DataContext as LiveCookingViewModel;
}
