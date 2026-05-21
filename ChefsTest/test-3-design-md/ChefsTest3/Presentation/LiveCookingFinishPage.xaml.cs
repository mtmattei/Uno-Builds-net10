using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class LiveCookingFinishPage : Page
{
    public LiveCookingFinishViewModel ViewModel => (LiveCookingFinishViewModel)DataContext;

    public LiveCookingFinishPage()
    {
        this.InitializeComponent();
    }
}
