using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HelpdeskApi.Data;
using HelpdeskApi.DTOs;
using HelpdeskApi.Models;

namespace HelpdeskApi.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    public ReportService(AppDbContext db) => _db = db;

    private static readonly string[] Headers =
        { "Ticket #", "Title", "Status", "Priority", "Category", "Requester", "Assignee", "Created", "Due", "Resolved", "Overdue" };

    // ── filtered ticket query (shared by all formats + preview) ─────────────────────
    private IQueryable<Ticket> Filtered(ReportFilter f)
    {
        var q = _db.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (f.From.HasValue) q = q.Where(t => t.CreatedAt >= f.From.Value.Date);
        if (f.To.HasValue) { var end = f.To.Value.Date.AddDays(1); q = q.Where(t => t.CreatedAt < end); }
        if (!string.IsNullOrWhiteSpace(f.Status) && Enum.TryParse<TicketStatus>(f.Status, out var st))
            q = q.Where(t => t.Status == st);
        if (!string.IsNullOrWhiteSpace(f.Priority) && Enum.TryParse<TicketPriority>(f.Priority, out var pr))
            q = q.Where(t => t.Priority == pr);
        if (f.CategoryId.HasValue) q = q.Where(t => t.CategoryId == f.CategoryId.Value);

        return q.OrderByDescending(t => t.CreatedAt);
    }

    // කී ticket එකක් match වෙනවද (empty download block කරන්න validate කරන්න)
    public int Count(ReportFilter filter) => Filtered(filter).Count();

    // table preview — paginated JSON
    public PagedResult<TicketListItemResponse> Preview(ReportFilter filter, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var q = Filtered(filter);
        var total = q.Count();
        var items = q.Skip((page - 1) * pageSize).Take(pageSize).ToList().Select(Map).ToList();

        return new PagedResult<TicketListItemResponse> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    private static TicketListItemResponse Map(Ticket t) => new()
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
        IsOverdue = IsOverdue(t)
    };

    private static bool IsOverdue(Ticket t) =>
        t.DueAt < DateTime.UtcNow && t.Status is not (TicketStatus.Resolved or TicketStatus.Closed);

    private static string[] Row(Ticket t) => new[]
    {
        t.TicketNumber,
        t.Title,
        t.Status.ToString(),
        t.Priority.ToString(),
        t.Category?.Name ?? "",
        t.CreatedBy?.FullName ?? "",
        t.AssignedTo?.FullName ?? "—",
        t.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
        t.DueAt.ToString("yyyy-MM-dd HH:mm"),
        t.ResolvedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
        IsOverdue(t) ? "Yes" : "No"
    };

    // ── A) CSV ────────────────────────────────────────────────────────────────────────
    public byte[] TicketsCsv(ReportFilter filter)
    {
        var tickets = Filtered(filter).ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Headers.Select(Esc)));
        foreach (var t in tickets)
            sb.AppendLine(string.Join(",", Row(t).Select(Esc)));

        // UTF-8 BOM → Excel එකේ unicode (සිංහල) නම් හරියට පෙන්නන්න
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

        static string Esc(string v) => "\"" + (v ?? "").Replace("\"", "\"\"") + "\"";
    }

    // ── summary aggregation (PDF + Excel දෙකටම) ─────────────────────────────────────
    private sealed class Summary
    {
        public int Total, Open, InProgress, OnHold, Resolved, Closed, Reopened, Overdue, Unassigned;
        public double AvgResolutionHours;
        public List<KeyValuePair<string, int>> ByStatus = new();
        public List<KeyValuePair<string, int>> ByPriority = new();
        public List<KeyValuePair<string, int>> ByCategory = new();
    }

    private static Summary Build(List<Ticket> tickets)
    {
        bool Openish(Ticket t) => t.Status is not (TicketStatus.Resolved or TicketStatus.Closed);
        var durations = tickets.Where(t => t.ResolvedAt != null)
            .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours).ToList();

        return new Summary
        {
            Total = tickets.Count,
            Open = tickets.Count(t => t.Status == TicketStatus.Open),
            InProgress = tickets.Count(t => t.Status == TicketStatus.InProgress),
            OnHold = tickets.Count(t => t.Status == TicketStatus.OnHold),
            Resolved = tickets.Count(t => t.Status == TicketStatus.Resolved),
            Closed = tickets.Count(t => t.Status == TicketStatus.Closed),
            Reopened = tickets.Count(t => t.Status == TicketStatus.Reopened),
            Overdue = tickets.Count(IsOverdue),
            Unassigned = tickets.Count(t => t.AssignedToId == null && Openish(t)),
            AvgResolutionHours = durations.Any() ? Math.Round(durations.Average(), 1) : 0,
            ByStatus = Enum.GetValues<TicketStatus>()
                .Select(s => new KeyValuePair<string, int>(s.ToString(), tickets.Count(t => t.Status == s)))
                .Where(x => x.Value > 0).ToList(),
            ByPriority = Enum.GetValues<TicketPriority>()
                .Select(p => new KeyValuePair<string, int>(p.ToString(), tickets.Count(t => t.Priority == p)))
                .Where(x => x.Value > 0).ToList(),
            ByCategory = tickets.GroupBy(t => t.Category?.Name ?? "—")
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Value).ToList()
        };
    }

    private static string RangeLabel(ReportFilter f)
    {
        var from = f.From?.ToString("yyyy-MM-dd") ?? "the beginning";
        var to = f.To?.ToString("yyyy-MM-dd") ?? "today";
        return (f.From == null && f.To == null) ? "All time" : $"{from} → {to}";
    }

    // ── C) PDF (QuestPDF — one-page summary) ────────────────────────────────────────
    public byte[] SummaryPdf(ReportFilter filter)
    {
        var tickets = Filtered(filter).ToList();
        var s = Build(tickets);
        var brand = "#2563eb";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor("#111827"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("HelpDesk — Summary Report").FontSize(18).Bold().FontColor(brand);
                            c.Item().Text($"Range: {RangeLabel(filter)}").FontSize(9).FontColor("#6b7280");
                        });
                        row.ConstantItem(160).AlignRight().Text($"Generated\n{DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(9).FontColor("#6b7280");
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#e5e7eb");
                });

                page.Content().PaddingVertical(14).Column(col =>
                {
                    col.Spacing(16);

                    // KPI grid (2 rows × up to 4)
                    col.Item().Element(e => KpiGrid(e, new()
                    {
                        new("Total tickets", s.Total.ToString()),
                        new("Open", s.Open.ToString()),
                        new("In progress", s.InProgress.ToString()),
                        new("On hold", s.OnHold.ToString()),
                        new("Resolved", s.Resolved.ToString()),
                        new("Closed", s.Closed.ToString()),
                        new("Overdue", s.Overdue.ToString()),
                        new("Unassigned", s.Unassigned.ToString()),
                        new("Avg resolution", $"{s.AvgResolutionHours} h"),
                    }));

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(e => Breakdown(e, "By status", s.ByStatus, s.Total, brand));
                        row.ConstantItem(16);
                        row.RelativeItem().Element(e => Breakdown(e, "By priority", s.ByPriority, s.Total, brand));
                    });

                    col.Item().Element(e => Breakdown(e, "By category", s.ByCategory, s.Total, brand));
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("HelpDesk report · ").FontSize(8).FontColor("#9ca3af");
                    x.CurrentPageNumber().FontSize(8).FontColor("#9ca3af");
                    x.Span(" / ").FontSize(8).FontColor("#9ca3af");
                    x.TotalPages().FontSize(8).FontColor("#9ca3af");
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static void KpiGrid(IContainer container, List<KeyValuePair<string, string>> kpis)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
            foreach (var kpi in kpis)
            {
                table.Cell().Padding(4).Border(0.75f).BorderColor("#e5e7eb").Padding(10).Column(cc =>
                {
                    cc.Item().Text(kpi.Key).FontSize(8).FontColor("#6b7280");
                    cc.Item().Text(kpi.Value).FontSize(18).Bold();
                });
            }
        });
    }

    private static void Breakdown(IContainer container, string title, List<KeyValuePair<string, int>> rows, int total, string brand)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(4).Text(title).Bold().FontColor(brand);
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(3); c.ConstantColumn(50); c.ConstantColumn(60); });
                table.Header(h =>
                {
                    h.Cell().Element(HeadCell).Text("Label");
                    h.Cell().Element(HeadCell).AlignRight().Text("Count");
                    h.Cell().Element(HeadCell).AlignRight().Text("Share");
                });
                foreach (var r in rows)
                {
                    var pct = total > 0 ? $"{Math.Round(100.0 * r.Value / total)}%" : "0%";
                    table.Cell().Element(BodyCell).Text(r.Key);
                    table.Cell().Element(BodyCell).AlignRight().Text(r.Value.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text(pct);
                }
                if (rows.Count == 0)
                    table.Cell().ColumnSpan(3).Element(BodyCell).Text("No data").FontColor("#9ca3af");
            });
        });

        static IContainer HeadCell(IContainer c) =>
            c.Background("#f3f4f6").PaddingVertical(5).PaddingHorizontal(6).DefaultTextStyle(x => x.FontSize(8).SemiBold());
        static IContainer BodyCell(IContainer c) =>
            c.BorderBottom(0.5f).BorderColor("#eceef1").PaddingVertical(4).PaddingHorizontal(6).DefaultTextStyle(x => x.FontSize(9));
    }

    // ── D) Excel (ClosedXML — Tickets + Summary sheets) ─────────────────────────────
    public byte[] FullXlsx(ReportFilter filter)
    {
        var tickets = Filtered(filter).ToList();
        var s = Build(tickets);

        using var wb = new XLWorkbook();

        // Sheet 1 — Tickets
        var ws = wb.Worksheets.Add("Tickets");
        for (int i = 0; i < Headers.Length; i++) ws.Cell(1, i + 1).Value = Headers[i];
        var head = ws.Row(1);
        head.Style.Font.Bold = true;
        head.Style.Fill.BackgroundColor = XLColor.FromHtml("#1f2937");
        head.Style.Font.FontColor = XLColor.White;

        var r = 2;
        foreach (var t in tickets)
        {
            var cells = Row(t);
            for (int i = 0; i < cells.Length; i++) ws.Cell(r, i + 1).Value = cells[i];
            r++;
        }
        if (tickets.Count > 0) ws.Range(1, 1, r - 1, Headers.Length).SetAutoFilter();
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();
        ws.Column(2).Width = Math.Min(ws.Column(2).Width, 45);   // Title column cap

        // Sheet 2 — Summary
        var sum = wb.Worksheets.Add("Summary");
        sum.Cell(1, 1).Value = "HelpDesk — Summary";
        sum.Cell(1, 1).Style.Font.Bold = true;
        sum.Cell(1, 1).Style.Font.FontSize = 14;
        sum.Cell(2, 1).Value = $"Range: {RangeLabel(filter)}";
        sum.Cell(3, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}";
        sum.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;

        var row = 5;
        void Kpi(string k, object v) { sum.Cell(row, 1).Value = k; sum.Cell(row, 2).Value = XLCellValue.FromObject(v); row++; }
        sum.Cell(row, 1).Value = "Metric"; sum.Cell(row, 2).Value = "Value";
        sum.Range(row, 1, row, 2).Style.Font.Bold = true; row++;
        Kpi("Total tickets", s.Total);
        Kpi("Open", s.Open);
        Kpi("In progress", s.InProgress);
        Kpi("On hold", s.OnHold);
        Kpi("Resolved", s.Resolved);
        Kpi("Closed", s.Closed);
        Kpi("Reopened", s.Reopened);
        Kpi("Overdue", s.Overdue);
        Kpi("Unassigned", s.Unassigned);
        Kpi("Avg resolution (hours)", s.AvgResolutionHours);

        row += 1;
        WriteBreakdown(sum, ref row, "By status", s.ByStatus, s.Total);
        row += 1;
        WriteBreakdown(sum, ref row, "By priority", s.ByPriority, s.Total);
        row += 1;
        WriteBreakdown(sum, ref row, "By category", s.ByCategory, s.Total);

        sum.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void WriteBreakdown(IXLWorksheet ws, ref int row, string title, List<KeyValuePair<string, int>> rows, int total)
    {
        ws.Cell(row, 1).Value = title;
        ws.Cell(row, 1).Style.Font.Bold = true; row++;
        ws.Cell(row, 1).Value = "Label"; ws.Cell(row, 2).Value = "Count"; ws.Cell(row, 3).Value = "Share";
        ws.Range(row, 1, row, 3).Style.Font.Bold = true; row++;
        foreach (var kv in rows)
        {
            ws.Cell(row, 1).Value = kv.Key;
            ws.Cell(row, 2).Value = kv.Value;
            ws.Cell(row, 3).Value = total > 0 ? $"{Math.Round(100.0 * kv.Value / total)}%" : "0%";
            row++;
        }
    }
}
