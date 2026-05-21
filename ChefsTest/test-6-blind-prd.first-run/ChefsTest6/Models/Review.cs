using System.Collections.ObjectModel;

namespace ChefsTest6.Models;

public partial class Review : ObservableObject
{
    public Guid Id { get; init; }
    public Guid RecipeId { get; init; }
    public string? UrlAuthorImage { get; init; }
    public Guid CreatedBy { get; init; }
    public string? PublisherName { get; init; }
    public DateTime Date { get; init; }
    public string? Description { get; init; }

    public ObservableCollection<Guid> Likes { get; } = new();
    public ObservableCollection<Guid> Dislikes { get; } = new();

    [ObservableProperty]
    private bool? userLike;

    public int LikeCount => Likes.Count;
    public int DislikeCount => Dislikes.Count;

    partial void OnUserLikeChanged(bool? value)
    {
        OnPropertyChanged(nameof(LikeCount));
        OnPropertyChanged(nameof(DislikeCount));
    }

    public void RaiseCounts()
    {
        OnPropertyChanged(nameof(LikeCount));
        OnPropertyChanged(nameof(DislikeCount));
    }
}
