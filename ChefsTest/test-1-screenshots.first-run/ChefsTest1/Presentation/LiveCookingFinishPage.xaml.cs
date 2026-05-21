using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class LiveCookingFinishPage : Page
{
    public LiveCookingFinishViewModel ViewModel => (LiveCookingFinishViewModel)DataContext;

    public LiveCookingFinishPage()
    {
        this.InitializeComponent();
    }
}
