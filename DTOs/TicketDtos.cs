using System.ComponentModel.DataAnnotations;
using ProductApi.Models;

namespace ProductApi.DTOs;

// ── Requests ─────────────────────────────────────────────────────────────────────

public class CreateTicketRequest
{
    [Required(ErrorMessage = "title අනිවාර්යයි")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "title අකුරු 3-150 අතර")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "description අනිවාර්යයි")]
    [StringLength(4000, MinimumLength = 5)]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "category එක තෝරන්න")]
    public int CategoryId { get; set; }

    // Customer ට "suggest" කරන්න පුළුවන්; දාන්නේ නැත්නම් category එකේ default එක ගනී.
    public TicketPriority? Priority { get; set; }
}

public class UpdateTicketRequest
{
    [Required][StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = "";

    [Required][StringLength(4000, MinimumLength = 5)]
    public string Description { get; set; } = "";

    [Required] public int CategoryId { get; set; }
    [Required] public TicketPriority Priority { get; set; }
}

public class ChangeStatusRequest
{
    [Required] public TicketStatus Status { get; set; }
}

public class AssignRequest
{
    // null = unassign; agent/admin userId එකක් = assign
    public int? AssignedToId { get; set; }
}

// ── Filter / query (list endpoint එකට) ────────────────────────────────────────────
public class TicketFilter
{
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public int? CategoryId { get; set; }
    public int? AssignedToId { get; set; }     // -1 => unassigned, null => any
    public bool? Mine { get; set; }             // agent: තමන්ට assign වුණ ඒවා; customer: අදාළ නෑ
    public string? Search { get; set; }         // title / ticket number / description
    public string? Sort { get; set; }           // newest | oldest | priority | due
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ── Responses ────────────────────────────────────────────────────────────────────

// List එකේ එක row එකක් (සැහැල්ලු — comments/history නෑ)
public class TicketListItemResponse
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string Status { get; set; } = "";
    public string Priority { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string CreatedByName { get; set; } = "";
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueAt { get; set; }
    public bool IsOverdue { get; set; }
}

// පිටුවකට ආපසු යවන list එක (paging එක්ක)
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
}

// Full detail (ticket detail page එකට)
public class TicketDetailResponse
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public string Priority { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = "";
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsOverdue { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
    public List<HistoryResponse> History { get; set; } = new();
}
