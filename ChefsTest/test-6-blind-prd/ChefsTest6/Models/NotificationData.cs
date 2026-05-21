namespace ChefsTest6.Models;

public partial class NotificationData : ObservableObject
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    [ObservableProperty]
    private bool isRead;

    public string RelativeBucket
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            var d = Date.ToUniversalTime().Date;
            if (d == today) return "Today";
            if (d == today.AddDays(-1)) return "Yesterday";
            if (d > today.AddDays(-7)) return d.ToString("dddd");
            return d.ToString("MMMM d");
        }
    }
}
