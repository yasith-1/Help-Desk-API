namespace ProductApi.DTOs;

// Dashboard එකේ stat cards + charts වලට අවශ්‍ය numbers.
public class DashboardStats
{
    // Top cards
    public int Total { get; set; }
    public int Open { get; set; }
    public int InProgress { get; set; }
    public int Resolved { get; set; }
    public int Overdue { get; set; }
    public int Unassigned { get; set; }
    public int ResolvedToday { get; set; }
    public int MyOpen { get; set; }              // current user ට අදාළ open ticket ගණන

    public double AvgResolutionHours { get; set; }   // සාමාන්‍ය resolve කරන කාලය (පැය)

    // Charts
    public List<CountByLabel> Trend { get; set; } = new();      // අන්තිම දවස් 14ේ tickets/day
    public List<CountByLabel> ByStatus { get; set; } = new();
    public List<CountByLabel> ByPriority { get; set; } = new();
    public List<CountByLabel> ByCategory { get; set; } = new();

    public List<TicketListItemResponse> Recent { get; set; } = new();
}

public class CountByLabel
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
}
