using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ChefsTest5.Presentation.Converters;

public sealed class IndexToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int idx && parameter is string param && int.TryParse(param, out var target))
        {
            return idx == target ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => null!;
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var v = value is bool b && b;
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase)) v = !v;
        return v ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => null!;
}

public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hasItems = value switch
        {
            int i => i > 0,
            System.Collections.ICollection c => c.Count > 0,
            System.Collections.IEnumerable e => e.GetEnumerator().MoveNext(),
            _ => false,
        };
        if (parameter is string s && s.Equals("empty", StringComparison.OrdinalIgnoreCase))
            return hasItems ? Visibility.Collapsed : Visibility.Visible;
        return hasItems ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => null!;
}

public sealed class IndexToBrushConverter : IValueConverter
{
    public Microsoft.UI.Xaml.Media.Brush? ActiveBrush { get; set; }
    public Microsoft.UI.Xaml.Media.Brush? InactiveBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int idx && parameter is string p && int.TryParse(p, out var target))
        {
            return idx == target ? ActiveBrush! : InactiveBrush!;
        }
        return InactiveBrush!;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => null!;
}
