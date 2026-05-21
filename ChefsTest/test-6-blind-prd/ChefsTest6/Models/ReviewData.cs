namespace ChefsTest6.Models;

public partial class ReviewData : ObservableObject
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public Guid CreatedBy { get; set; }
    public string? UrlAuthorImage { get; set; }
    public string? PublisherName { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public List<Guid>? Likes { get; set; }
    public List<Guid>? Dislikes { get; set; }
    public bool? UserLike { get; set; }

    [ObservableProperty]
    private int likeCount;

    [ObservableProperty]
    private int dislikeCount;

    [ObservableProperty]
    private bool isLiked;

    [ObservableProperty]
    private bool isDisliked;
}
