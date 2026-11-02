using HelpdeskApi.DTOs;

namespace HelpdeskApi.Services;

// Admin reports — Tickets (CSV), Summary (PDF), Full workbook (Excel). byte[] → controller File().
public interface IReportService
{
    byte[] TicketsCsv(ReportFilter filter);
    byte[] SummaryPdf(ReportFilter filter);
    byte[] FullXlsx(ReportFilter filter);

    int Count(ReportFilter filter);                                              // empty download block කරන්න
    PagedResult<TicketListItemResponse> Preview(ReportFilter filter, int page, int pageSize);  // table preview
}
