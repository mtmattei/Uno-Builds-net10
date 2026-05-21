using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ChefsTest1.Presentation;

public sealed partial class RecipeTabHeader : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(RecipeTabHeader),
            new PropertyMetadata("", (d, _) => ((RecipeTabHeader)d).Update()));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(RecipeTabHeader),
            new PropertyMetadata(false, (d, _) => ((RecipeTabHeader)d).Update()));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(RecipeTabHeader),
            new PropertyMetadata(null));

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public bool IsActive { get => (bool)GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

    public RecipeTabHeader()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => Update();
    }

    private void Update()
    {
        LabelText.Text = Label;
        if (IsActive)
        {
            LabelText.Foreground = (Brush?)Application.Current.Resources["PrimaryBrush"];
            LabelText.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
            Underline.Background = (Brush?)Application.Current.Resources["PrimaryBrush"];
        }
        else
        {
            LabelText.Foreground = (Brush?)Application.Current.Resources["MutedTextBrush"];
            LabelText.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
            Underline.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (Command?.CanExecute(null) == true)
            Command.Execute(null);
    }
}
