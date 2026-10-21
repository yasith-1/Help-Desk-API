using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IPermissionService _perms;
    public DashboardService(AppDbContext db, IPermissionService perms) { _db = db; _perms = perms; }

    public DashboardStats GetStats(int userId, string role)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var canViewAll = _perms.Has(role, Permissions.TicketsViewAll);

        // viewAll නැත්නම් තමන්ගේ ticket; තියෙනවා නම් ඔක්කොම
        var baseQuery = _db.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!canViewAll)
            baseQuery = baseQuery.Where(t => t.CreatedById == userId);

        var tickets = baseQuery.ToList();   // dataset කුඩා — memory එකේ aggregate

        bool IsOpenish(Ticket t) => t.Status is not (TicketStatus.Resolved or TicketStatus.Closed);
        bool Overdue(Ticket t) => t.DueAt < now && IsOpenish(t);

        var resolvedDurations = tickets
            .Where(t => t.ResolvedAt != null)
            .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            .ToList();

        var stats = new DashboardStats
        {
            Total = tickets.Count,
            Open = tickets.Count(t => t.Status == TicketStatus.Open),
            InProgress = tickets.Count(t => t.Status == TicketStatus.InProgress),
            Resolved = tickets.Count(t => t.Status is TicketStatus.Resolved or TicketStatus.Closed),
            Overdue = tickets.Count(Overdue),
            Unassigned = tickets.Count(t => t.AssignedToId == null && IsOpenish(t)),
            ResolvedToday = tickets.Count(t => t.ResolvedAt != null && t.ResolvedAt.Value >= todayStart),
            MyOpen = canViewAll
                ? tickets.Count(t => t.AssignedToId == userId && IsOpenish(t))
                : tickets.Count(IsOpenish),
            AvgResolutionHours = resolvedDurations.Any() ? Math.Round(resolvedDurations.Average(), 1) : 0,

            Trend = Enumerable.Range(0, 14)
                .Select(i => todayStart.AddDays(-13 + i))
                .Select(day => new CountByLabel
                {
                    Label = day.ToString("MMM d"),
                    Count = tickets.Count(t => t.CreatedAt.Date == day)
                })
                .ToList(),

            ByStatus = Enum.GetValues<TicketStatus>()
                .Select(s => new CountByLabel { Label = s.ToString(), Count = tickets.Count(t => t.Status == s) })
                .Where(x => x.Count > 0)
                .ToList(),

            ByPriority = Enum.GetValues<TicketPriority>()
                .Select(p => new CountByLabel { Label = p.ToString(), Count = tickets.Count(t => t.Priority == p) })
                .Where(x => x.Count > 0)
                .ToList(),

            ByCategory = tickets
                .GroupBy(t => t.Category?.Name ?? "—")
                .Select(g => new CountByLabel { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),

            Recent = tickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(6)
                .Select(t => new TicketListItemResponse
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Title = t.Title,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    CategoryName = t.Category?.Name ?? "",
                    CreatedByName = t.CreatedBy?.FullName ?? "",
                    AssignedToName = t.AssignedTo?.FullName,
                    CreatedAt = t.CreatedAt,
                    DueAt = t.DueAt,
                    IsOverdue = Overdue(t)
                })
                .ToList()
        };

        return stats;
    }
}
