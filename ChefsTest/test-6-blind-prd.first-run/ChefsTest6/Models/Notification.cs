namespace ChefsTest6.Models;

public partial class Notification : ObservableObject
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime Date { get; init; }

    [ObservableProperty]
    private bool isRead;
}
