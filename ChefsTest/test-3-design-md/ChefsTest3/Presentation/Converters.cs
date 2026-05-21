using System;
using ChefsTest3.Models;
using Microsoft.UI.Xaml;

namespace ChefsTest3.Presentation;

public static class BindableHelpers
{
    public static Visibility CountToVisibility(int count) => count > 0 ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility CountToEmpty(int count) => count > 0 ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility BoolToCollapsed(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    public static string FormatRecipeCount(int count) => count == 1 ? $"{count} recipe" : $"{count} recipes";
    public static string FormatCookbookCount(int count) => count == 1 ? $"{count} cookbook" : $"{count} cookbooks";
    public static string FormatNotificationCount(int count) => count == 1 ? $"{count} notification" : $"{count} notifications";

    public static string FormatCookTime(TimeSpan ts)
    {
        if (ts.TotalMinutes < 1) return "<1 min";
        if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        return $"{(int)ts.TotalMinutes} min";
    }

    public static string DifficultyLabel(int difficulty) => difficulty switch
    {
        0 => "Easy",
        1 => "Medium",
        _ => "Hard",
    };

    public static string Heart(bool isFavorite) => isFavorite ? "♥" : "♡";

    public static string FormatRelativeDate(DateTime date)
    {
        var diff = DateTime.UtcNow - date.ToUniversalTime();
        if (diff.TotalDays > 365) return $"{(int)(diff.TotalDays / 365)}y ago";
        if (diff.TotalDays > 30) return $"{(int)(diff.TotalDays / 30)}mo ago";
        if (diff.TotalDays > 1) return $"{(int)diff.TotalDays}d ago";
        if (diff.TotalHours > 1) return $"{(int)diff.TotalHours}h ago";
        return "Just now";
    }

    public static string FormatLong(long? value) => (value ?? 0).ToString("N0");
}
