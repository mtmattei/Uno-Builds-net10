using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ChefsTest2.Presentation;

public sealed class BoolToVisibilityConverter : IValueConverter
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

public sealed class CountToVisibilityConverter : IValueConverter
{
    public bool ShowWhenZero { get; set; }
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var count = value is int i ? i : (value is long l ? (int)l : 0);
        if (value is System.Collections.ICollection c) count = c.Count;
        var show = ShowWhenZero ? count == 0 : count > 0;
        return show ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class TimeSpanToMinutesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TimeSpan ts) return $"{(int)ts.TotalMinutes} min";
        return string.Empty;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class DifficultyToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is int d ? d switch
        {
            0 => "Easy",
            1 => "Medium",
            2 => "Hard",
            _ => "—",
        } : "—";
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                var trimmed = hex.TrimStart('#');
                if (trimmed.Length == 6)
                {
                    var r = byte.Parse(trimmed.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var g = byte.Parse(trimmed.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    var b = byte.Parse(trimmed.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, r, g, b));
                }
            }
            catch { }
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x7A, 0x67, 0xF8));
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public sealed class RelativeDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        DateTimeOffset dt;
        if (value is DateTimeOffset d) dt = d;
        else if (value is DateTime dn) dt = dn;
        else return string.Empty;
        var span = DateTimeOffset.UtcNow - dt;
        if (span.TotalDays < 1) return "Today";
        if (span.TotalDays < 2) return "Yesterday";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";
        if (span.TotalDays < 30) return $"{(int)(span.TotalDays / 7)} weeks ago";
        if (span.TotalDays < 365) return $"{(int)(span.TotalDays / 30)} months ago";
        return dt.ToString("MMM yyyy");
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
