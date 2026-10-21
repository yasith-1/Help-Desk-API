namespace ProductApi.Models;

// Audit trail — ticket එකකට වුණු හැම වෙනසක්ම (status මාරුව, assign, priority...) log කරනවා.
// Timeline එකක් විදිහට ticket detail page එකේ පෙන්නනවා.
public class TicketHistory
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public int ActorId { get; set; }                    // වෙනස කරපු user
    public User? Actor { get; set; }

    public string Action { get; set; } = "";            // උදා: "Status: Open → InProgress"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
