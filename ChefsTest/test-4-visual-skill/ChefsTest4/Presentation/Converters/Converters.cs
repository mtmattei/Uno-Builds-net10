using System;
using System.Collections;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ChefsTest4.Presentation.Converters;

// DESIGN.md / WinUI cannot use Binding StringFormat — converters fill the gap (memory: WinUI no StringFormat).

public sealed class CookTimeMinutesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TimeSpan ts)
        {
            var min = (int)Math.Round(ts.TotalMinutes);
            return min <= 0 ? "—" : $"{min} min";
        }
        return string.Empty;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class DifficultyLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is int d
            ? d switch { 0 => "Easy", 1 => "Medium", _ => "Advanced" }
            : "Easy";
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class CountResultsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var n = value switch
        {
            int i => i,
            long l => (int)l,
            ICollection c => c.Count,
            _ => 0,
        };
        return $"{n} {parameter ?? "results"}";
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var b = value is bool bb && bb;
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        if (invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var visibleWhenNull = string.Equals(parameter as string, "WhenNull", StringComparison.OrdinalIgnoreCase);
        var isNull = value is null || (value is string s && string.IsNullOrEmpty(s));
        return (isNull == visibleWhenNull) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class ListEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var visibleWhenEmpty = string.Equals(parameter as string, "WhenEmpty", StringComparison.OrdinalIgnoreCase);
        var empty = value is null || (value is ICollection c && c.Count == 0);
        return (empty == visibleWhenEmpty) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class HeartGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool b && b) ? "" : ""; // Filled vs outlined heart (Segoe MDL2)
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class HeartBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var key = (value is bool b && b) ? "HeartActiveBrush" : "HeartInactiveBrush";
        if (Application.Current.Resources.TryGetValue(key, out var brush) && brush is Brush br) return br;
        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && hex.StartsWith('#') && (hex.Length == 7 || hex.Length == 9))
        {
            try
            {
                byte a = 0xFF, r, g, b;
                if (hex.Length == 7)
                {
                    r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                    g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                    b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                }
                else
                {
                    a = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                    r = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                    g = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                    b = byte.Parse(hex.Substring(7, 2), NumberStyles.HexNumber);
                }
                return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            }
            catch { }
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class RelativeDateGroupConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            var now = DateTime.Today;
            var date = dt.Date;
            var days = (now - date).Days;
            if (days <= 0) return "Today";
            if (days == 1) return "Yesterday";
            if (days < 7) return date.ToString("dddd", CultureInfo.InvariantCulture);
            return date.ToString("MMM d", CultureInfo.InvariantCulture);
        }
        return string.Empty;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class StarFilledConverter : IValueConverter
{
    // value = current rating (int), parameter = star ordinal "1".."5"
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int rating && parameter is string p && int.TryParse(p, out var ord))
        {
            return ord <= rating ? "" : ""; // Filled vs outlined star (Segoe MDL2)
        }
        return "";
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class GreaterThanZeroToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            int i => i > 0,
            long l => l > 0,
            ICollection c => c.Count > 0,
            _ => false,
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is not null;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
