namespace HelpdeskApi.DTOs;

// Report generate කරද්දී filter කරන්න (date range + status/priority/category). හැමෝම optional.
public class ReportFilter
{
    public DateTime? From { get; set; }        // CreatedAt >= From (inclusive)
    public DateTime? To { get; set; }          // CreatedAt <= To (end-of-day inclusive)
    public string? Status { get; set; }        // "Open", "InProgress"… (null/empty = all)
    public string? Priority { get; set; }      // "Low"…"Urgent" (null/empty = all)
    public int? CategoryId { get; set; }       // null = all categories
}
