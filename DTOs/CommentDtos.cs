using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

public class CreateCommentRequest
{
    [Required(ErrorMessage = "message එක හිස් වෙන්න බෑ")]
    [StringLength(4000, MinimumLength = 1)]
    public string Body { get; set; } = "";

    // true = internal note (agents/admin ට විතරක්). Customer ට මේක true කරන්න දෙන්නේ නෑ.
    public bool IsInternal { get; set; } = false;
}

public class CommentResponse
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = "";
    public string AuthorRole { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HistoryResponse
{
    public string ActorName { get; set; } = "";
    public string Action { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
