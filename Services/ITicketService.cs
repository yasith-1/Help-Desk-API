using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

// Ticket business logic. හැම method එකකටම current user (id + role) දෙනවා — scoping/permission වලට.
public interface ITicketService
{
    PagedResult<TicketListItemResponse> GetTickets(TicketFilter filter, int userId, string role);
    TicketDetailResponse GetById(int id, int userId, string role);
    TicketDetailResponse Create(CreateTicketRequest request, int userId, string role);
    TicketDetailResponse Update(int id, UpdateTicketRequest request, int userId, string role);
    TicketDetailResponse ChangeStatus(int id, TicketStatus status, int userId, string role);
    TicketDetailResponse Assign(int id, int? assignedToId, int userId, string role);
    CommentResponse AddComment(int id, CreateCommentRequest request, int userId, string role);
    List<HistoryResponse> GetHistory(int id, int userId, string role);
}
