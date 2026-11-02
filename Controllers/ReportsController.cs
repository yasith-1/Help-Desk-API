using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HelpdeskApi.DTOs;
using HelpdeskApi.Models;
using HelpdeskApi.Services;

namespace HelpdeskApi.Controllers;

// Admin reports — download tickets (CSV), summary (PDF), full workbook (Excel).
// හැම endpoint එකක්ම "reports.export" permission එකෙන් protect — admin ට විතරයි (default).
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reports;
    public ReportsController(IReportService reports) => _reports = reports;

    // GET /api/reports/tickets-csv?from=&to=&status=&priority=&categoryId=
    [HttpGet("tickets-csv")]
    public IActionResult TicketsCsv([FromQuery] ReportFilter filter)
    {
        RequirePermission(Permissions.ReportsExport);
        return File(_reports.TicketsCsv(filter), "text/csv", Name("tickets", "csv"));
    }

    // GET /api/reports/summary-pdf?from=&to=&status=&priority=&categoryId=
    [HttpGet("summary-pdf")]
    public IActionResult SummaryPdf([FromQuery] ReportFilter filter)
    {
        RequirePermission(Permissions.ReportsExport);
        return File(_reports.SummaryPdf(filter), "application/pdf", Name("summary", "pdf"));
    }

    // GET /api/reports/full-excel?from=&to=&status=&priority=&categoryId=
    [HttpGet("full-excel")]
    public IActionResult FullExcel([FromQuery] ReportFilter filter)
    {
        RequirePermission(Permissions.ReportsExport);
        return File(_reports.FullXlsx(filter),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Name("report", "xlsx"));
    }

    private static string Name(string kind, string ext) => $"helpdesk-{kind}-{DateTime.Now:yyyyMMdd-HHmm}.{ext}";
}
