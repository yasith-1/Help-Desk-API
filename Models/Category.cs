namespace ProductApi.Models;

// Ticket වර්ගීකරණය (උදා: "IT / Hardware", "Billing", "General"). Admin ට manage කරන්න පුළුවන්.
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public TicketPriority DefaultPriority { get; set; } = TicketPriority.Medium;
    public int SlaHours { get; set; } = 24;   // මේ category එකට විසඳන්න දෙන පැය ගණන (SLA)

    // Soft delete — physically මකන්නේ නෑ, flag එකක් විතරයි (recover කරන්න පුළුවන්)
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public List<Ticket> Tickets { get; set; } = new();
}
