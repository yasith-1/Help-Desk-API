namespace ProductApi.Models;

// Ticket එකක conversation එකේ එක message එකක්.
// IsInternal = true නම් → agents/admin ට විතරක් පේන "internal note" එකක් (customer ට පේන්නේ නෑ).
public class TicketComment
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public string Body { get; set; } = "";
    public bool IsInternal { get; set; } = false;       // public reply (false) හෝ internal note (true)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
