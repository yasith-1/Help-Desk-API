using HelpdeskApi.DTOs;

namespace HelpdeskApi.Services;

public interface IDashboardService
{
    // Role-aware — Customer ට තමන්ගේ stats, staff ට මුළු system එකේ stats.
    DashboardStats GetStats(int userId, string role);
}
