namespace ProductApi.Models;

// මූලික entity — raise කරන ප්‍රශ්නය (ticket). System එකේ හදවත.
public class Ticket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = "";    // මිනිසුන්ට කියවන්න පුළුවන් අංකය — උදා: TKT-00001

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    // Category (FK)
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // කවුද raise කලේ (reporter — FK)
    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    // කාටද assign කරලා තියෙන්නේ (agent — FK, null = තාම assign කරලා නෑ)
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    // කාල දත්ත
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueAt { get; set; }                 // SLA deadline (CreatedAt + SLA hours)
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // සම්බන්ධ data
    public List<TicketComment> Comments { get; set; } = new();
    public List<TicketHistory> History { get; set; } = new();
}
