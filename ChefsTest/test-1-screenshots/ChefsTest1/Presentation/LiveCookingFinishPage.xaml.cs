namespace ChefsTest1.Presentation;

public sealed partial class LiveCookingFinishPage : Page
{
    public LiveCookingFinishPage()
    {
        this.InitializeComponent();
    }

    public LiveCookingFinishViewModel? ViewModel => DataContext as LiveCookingFinishViewModel;
}
