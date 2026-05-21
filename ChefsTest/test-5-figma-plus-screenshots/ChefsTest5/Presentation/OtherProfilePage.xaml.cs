namespace ChefsTest5.Presentation;

public sealed partial class OtherProfilePage : Page
{
    public OtherProfileViewModel ViewModel { get; private set; } = default!;

    public OtherProfilePage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is OtherProfileViewModel vm) ViewModel = vm; };
    }
}
