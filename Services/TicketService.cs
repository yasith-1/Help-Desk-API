using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

// Ticket business logic layer. Permission/scoping මෙතන බලනවා (PermissionService හරහා).
public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly IPermissionService _perms;

    public TicketService(AppDbContext db, IPermissionService perms)
    {
        _db = db;
        _perms = perms;
    }

    // Related data එක්ක ticket query එකක්
    private IQueryable<Ticket> Query() =>
        _db.Tickets
           .Include(t => t.Category)
           .Include(t => t.CreatedBy)
           .Include(t => t.AssignedTo);

    // ── List (filter + search + sort + paging + role scoping) ──────────────────────
    public PagedResult<TicketListItemResponse> GetTickets(TicketFilter f, int userId, string role)
    {
        var q = Query();
        var canViewAll = _perms.Has(role, Permissions.TicketsViewAll);

        // Scoping — viewAll නැත්නම් තමන්ගේ ticket විතරයි
        if (!canViewAll)
            q = q.Where(t => t.CreatedById == userId);
        else if (f.Mine == true)
            q = q.Where(t => t.AssignedToId == userId);

        if (f.Status.HasValue) q = q.Where(t => t.Status == f.Status.Value);
        if (f.Priority.HasValue) q = q.Where(t => t.Priority == f.Priority.Value);
        if (f.CategoryId.HasValue) q = q.Where(t => t.CategoryId == f.CategoryId.Value);
        if (f.AssignedToId.HasValue)
            q = f.AssignedToId.Value == -1
                ? q.Where(t => t.AssignedToId == null)
                : q.Where(t => t.AssignedToId == f.AssignedToId.Value);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = f.Search.Trim();
            q = q.Where(t => t.Title.Contains(s) || t.TicketNumber.Contains(s) || t.Description.Contains(s));
        }

        var total = q.Count();
        var page = f.Page < 1 ? 1 : f.Page;
        var size = f.PageSize is < 1 or > 100 ? 10 : f.PageSize;

        List<Ticket> rows;
        if (f.Sort == "priority")
        {
            rows = q.ToList()
                    .OrderByDescending(t => (int)t.Priority).ThenByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * size).Take(size).ToList();
        }
        else
        {
            q = f.Sort switch
            {
                "oldest" => q.OrderBy(t => t.CreatedAt),
                "due"    => q.OrderBy(t => t.DueAt),
                _        => q.OrderByDescending(t => t.CreatedAt),
            };
            rows = q.Skip((page - 1) * size).Take(size).ToList();
        }

        return new PagedResult<TicketListItemResponse>
        {
            Items = rows.Select(ToListItem).ToList(),
            Total = total, Page = page, PageSize = size
        };
    }

    // ── Get one (full detail) ──────────────────────────────────────────────────────
    public TicketDetailResponse GetById(int id, int userId, string role)
    {
        var t = Query()
            .Include(x => x.Comments).ThenInclude(c => c.Author)
            .Include(x => x.History).ThenInclude(h => h.Actor)
            .FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Ticket {id} හමු නොවීය");

        EnsureCanView(t, userId, role);
        return ToDetail(t, CanSeeInternal(role));
    }

    // ── Create ─────────────────────────────────────────────────────────────────────
    public TicketDetailResponse Create(CreateTicketRequest req, int userId, string role)
    {
        var category = _db.Categories.Find(req.CategoryId)
            ?? throw new ArgumentException("වලංගු category එකක් තෝරන්න");

        var now = DateTime.UtcNow;
        var priority = req.Priority ?? category.DefaultPriority;

        var ticket = new Ticket
        {
            Title = req.Title, Description = req.Description, CategoryId = category.Id,
            Priority = priority, Status = TicketStatus.Open, CreatedById = userId,
            CreatedAt = now, UpdatedAt = now, DueAt = now.AddHours(category.SlaHours)
        };
        ticket.History.Add(new TicketHistory { ActorId = userId, Action = "Ticket created", CreatedAt = now });

        _db.Tickets.Add(ticket);
        _db.SaveChanges();

        ticket.TicketNumber = $"TKT-{ticket.Id:D5}";
        _db.SaveChanges();

        return GetById(ticket.Id, userId, role);
    }

    // ── Update ─────────────────────────────────────────────────────────────────────
    public TicketDetailResponse Update(int id, UpdateTicketRequest req, int userId, string role)
    {
        var t = Query().FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Ticket {id} හමු නොවීය");

        if (!_perms.Has(role, Permissions.TicketsEditAny) && t.CreatedById != userId)
            throw new UnauthorizedAccessException("මේ ticket එක වෙනස් කරන්න ඔයාට අවසර නෑ");

        if (_db.Categories.Find(req.CategoryId) == null)
            throw new ArgumentException("වලංගු category එකක් තෝරන්න");

        t.Title = req.Title; t.Description = req.Description;
        t.CategoryId = req.CategoryId; t.Priority = req.Priority;
        t.UpdatedAt = DateTime.UtcNow;
        _db.TicketHistory.Add(new TicketHistory { TicketId = t.Id, ActorId = userId, Action = "Ticket details updated" });
        _db.SaveChanges();

        return GetById(id, userId, role);
    }

    // ── Change status ──────────────────────────────────────────────────────────────
    public TicketDetailResponse ChangeStatus(int id, TicketStatus status, int userId, string role)
    {
        var t = Query().FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Ticket {id} හමු නොවීය");

        if (!_perms.Has(role, Permissions.TicketsChangeStatus))
        {
            // perm නෑ: තමන්ගේ resolved/closed ticket එකක් reopen කරන්න විතරක් පුළුවන්
            if (t.CreatedById != userId)
                throw new UnauthorizedAccessException("මේ ticket එක වෙනස් කරන්න ඔයාට අවසර නෑ");
            if (!(status == TicketStatus.Reopened && t.Status is TicketStatus.Resolved or TicketStatus.Closed))
                throw new UnauthorizedAccessException("ඔයාට reopen කරන්න විතරයි පුළුවන්");
        }

        if (t.Status == status) return GetById(id, userId, role);

        var old = t.Status;
        t.Status = status;
        t.UpdatedAt = DateTime.UtcNow;
        if (status == TicketStatus.Resolved) t.ResolvedAt = DateTime.UtcNow;
        if (status == TicketStatus.Closed) t.ClosedAt = DateTime.UtcNow;
        if (status is TicketStatus.Reopened or TicketStatus.InProgress or TicketStatus.Open)
        {
            t.ResolvedAt = null; t.ClosedAt = null;
        }

        _db.TicketHistory.Add(new TicketHistory { TicketId = t.Id, ActorId = userId, Action = $"Status: {old} → {status}" });
        _db.SaveChanges();
        return GetById(id, userId, role);
    }

    // ── Assign ─────────────────────────────────────────────────────────────────────
    public TicketDetailResponse Assign(int id, int? assignedToId, int userId, string role)
    {
        _perms.Require(role, Permissions.TicketsAssign);

        var t = Query().FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Ticket {id} හමු නොවීය");

        string actionText;
        if (assignedToId == null)
        {
            t.AssignedToId = null; actionText = "Unassigned";
        }
        else
        {
            var agent = _db.Users.Find(assignedToId.Value)
                ?? throw new ArgumentException("User හමු නොවීය");
            if (!_perms.Has(agent.Role, Permissions.TicketsViewAll))
                throw new ArgumentException("Assign කරන්න පුළුවන් tickets handle කරන අයට විතරයි");
            t.AssignedToId = agent.Id;
            actionText = $"Assigned to {agent.FullName}";
        }

        t.UpdatedAt = DateTime.UtcNow;
        _db.TicketHistory.Add(new TicketHistory { TicketId = t.Id, ActorId = userId, Action = actionText });
        _db.SaveChanges();
        return GetById(id, userId, role);
    }

    // ── Add comment / internal note ────────────────────────────────────────────────
    public CommentResponse AddComment(int id, CreateCommentRequest req, int userId, string role)
    {
        var t = Query().FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Ticket {id} හමු නොවීය");

        EnsureCanView(t, userId, role);

        var isInternal = req.IsInternal && _perms.Has(role, Permissions.TicketsCommentInternal);

        var comment = new TicketComment
        {
            TicketId = t.Id, AuthorId = userId, Body = req.Body,
            IsInternal = isInternal, CreatedAt = DateTime.UtcNow
        };
        t.UpdatedAt = DateTime.UtcNow;
        _db.TicketComments.Add(comment);
        _db.SaveChanges();

        var author = _db.Users.Find(userId)!;
        return ToComment(comment, author);
    }

    // ── History ────────────────────────────────────────────────────────────────────
    public List<HistoryResponse> GetHistory(int id, int userId, string role)
    {
        var t = Query()
            .Include(x => x.History).ThenInclude(h => h.Actor)
            .FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Ticket {id} හමු නොවීය");

        EnsureCanView(t, userId, role);
        return t.History.OrderBy(h => h.CreatedAt)
                        .Select(h => new HistoryResponse
                        {
                            ActorName = h.Actor?.FullName ?? "System",
                            Action = h.Action, CreatedAt = h.CreatedAt
                        }).ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────────
    private bool CanSeeInternal(string role) => _perms.Has(role, Permissions.TicketsCommentInternal);

    private void EnsureCanView(Ticket t, int userId, string role)
    {
        if (!_perms.Has(role, Permissions.TicketsViewAll) && t.CreatedById != userId)
            throw new UnauthorizedAccessException("මේ ticket එක බලන්න ඔයාට අවසර නෑ");
    }

    private static bool Overdue(Ticket t) =>
        t.DueAt < DateTime.UtcNow && t.Status is not (TicketStatus.Resolved or TicketStatus.Closed);

    private static TicketListItemResponse ToListItem(Ticket t) => new()
    {
        Id = t.Id, TicketNumber = t.TicketNumber, Title = t.Title,
        Status = t.Status.ToString(), Priority = t.Priority.ToString(),
        CategoryName = t.Category?.Name ?? "", CreatedByName = t.CreatedBy?.FullName ?? "",
        AssignedToName = t.AssignedTo?.FullName, CreatedAt = t.CreatedAt,
        DueAt = t.DueAt, IsOverdue = Overdue(t)
    };

    private static TicketDetailResponse ToDetail(Ticket t, bool canSeeInternal)
    {
        var comments = t.Comments.AsEnumerable();
        if (!canSeeInternal) comments = comments.Where(c => !c.IsInternal);

        return new TicketDetailResponse
        {
            Id = t.Id, TicketNumber = t.TicketNumber, Title = t.Title, Description = t.Description,
            Status = t.Status.ToString(), Priority = t.Priority.ToString(),
            CategoryId = t.CategoryId, CategoryName = t.Category?.Name ?? "",
            CreatedById = t.CreatedById, CreatedByName = t.CreatedBy?.FullName ?? "",
            AssignedToId = t.AssignedToId, AssignedToName = t.AssignedTo?.FullName,
            CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt, DueAt = t.DueAt,
            ResolvedAt = t.ResolvedAt, ClosedAt = t.ClosedAt, IsOverdue = Overdue(t),
            Comments = comments.OrderBy(c => c.CreatedAt).Select(c => ToComment(c, c.Author)).ToList(),
            History = t.History.OrderBy(h => h.CreatedAt).Select(h => new HistoryResponse
            {
                ActorName = h.Actor?.FullName ?? "System", Action = h.Action, CreatedAt = h.CreatedAt
            }).ToList()
        };
    }

    private static CommentResponse ToComment(TicketComment c, User? author) => new()
    {
        Id = c.Id, AuthorId = c.AuthorId, AuthorName = author?.FullName ?? "",
        AuthorRole = author?.Role ?? "", Body = c.Body, IsInternal = c.IsInternal, CreatedAt = c.CreatedAt
    };
}
