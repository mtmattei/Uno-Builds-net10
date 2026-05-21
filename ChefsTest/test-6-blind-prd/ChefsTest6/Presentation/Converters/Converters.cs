using Microsoft.UI.Xaml.Data;

namespace ChefsTest6.Presentation.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var b = value is bool x && x;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var present = value is not null;
        if (Invert) present = !present;
        return present ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class CountToVisibilityConverter : IValueConverter
{
    public bool ShowWhenEmpty { get; set; }
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var count = value switch
        {
            int i => i,
            long l => (int)l,
            System.Collections.ICollection c => c.Count,
            _ => 0
        };
        var empty = count == 0;
        return empty == ShowWhenEmpty ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class HeartGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool b && b) ? "" : ""; // Segoe heart filled / outline
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class IsActiveToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (Microsoft.UI.Xaml.Application.Current.Resources.TryGetValue("PrimaryBrush", out var primary)
            && Microsoft.UI.Xaml.Application.Current.Resources.TryGetValue("OnSurfaceVariantBrush", out var muted))
        {
            return (value is bool b && b) ? primary : muted;
        }
        return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class IntEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var i = value is int v ? v : 0;
        var p = parameter is string s && int.TryParse(s, out var pi) ? pi : 0;
        return i == p;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class StarFillConverter : IValueConverter
{
    // Compares item index (parameter) with current rating (value). Returns true if filled.
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var rating = value is int v ? v : 0;
        var index = parameter is string s && int.TryParse(s, out var p) ? p : 0;
        return rating >= index;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
