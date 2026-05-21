using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ChefsTest6.Presentation;

// ConverterParameter format: "TrueBrushKey|FalseBrushKey".
// Looks brushes up in Application.Current.Resources at convert time so we get
// theme-aware values (Light/Dark) via the active MaterialToolkitTheme.
public sealed class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var keys = (parameter as string ?? string.Empty).Split('|');
        var key = (value is bool b && b)
            ? keys.ElementAtOrDefault(0)
            : keys.ElementAtOrDefault(1);

        if (!string.IsNullOrEmpty(key) &&
            Application.Current.Resources.TryGetValue(key!, out var resource) &&
            resource is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var b = value is bool x && x;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class CountToBoolConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var has = value is int n && n > 0;
        return Invert ? !has : has;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class NegateBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is bool b ? !b : true;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is bool b ? !b : false;
}
