using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]                         // login ඕන (ඕනම role)
public class TicketsController : ApiControllerBase
{
    private readonly ITicketService _service;
    public TicketsController(ITicketService service) => _service = service;

    // GET /api/tickets?status=&priority=&categoryId=&assignedTo=&mine=&search=&sort=&page=&pageSize=
    [HttpGet]
    public IActionResult GetAll([FromQuery] TicketFilter filter) =>
        Ok(_service.GetTickets(filter, CurrentUserId, CurrentRole));

    // GET /api/tickets/5
    [HttpGet("{id}")]
    public IActionResult GetOne(int id) =>
        Ok(_service.GetById(id, CurrentUserId, CurrentRole));

    // POST /api/tickets
    [HttpPost]
    public IActionResult Create([FromBody] CreateTicketRequest request)
    {
        var created = _service.Create(request, CurrentUserId, CurrentRole);
        return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
    }

    // PUT /api/tickets/5
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateTicketRequest request) =>
        Ok(_service.Update(id, request, CurrentUserId, CurrentRole));

    // PATCH /api/tickets/5/status
    [HttpPatch("{id}/status")]
    public IActionResult ChangeStatus(int id, [FromBody] ChangeStatusRequest request) =>
        Ok(_service.ChangeStatus(id, request.Status, CurrentUserId, CurrentRole));

    // PATCH /api/tickets/5/assign  (tickets.assign permission — service එකේ enforce වෙනවා)
    [HttpPatch("{id}/assign")]
    public IActionResult Assign(int id, [FromBody] AssignRequest request) =>
        Ok(_service.Assign(id, request.AssignedToId, CurrentUserId, CurrentRole));

    // POST /api/tickets/5/comments
    [HttpPost("{id}/comments")]
    public IActionResult AddComment(int id, [FromBody] CreateCommentRequest request) =>
        Ok(_service.AddComment(id, request, CurrentUserId, CurrentRole));

    // GET /api/tickets/5/history
    [HttpGet("{id}/history")]
    public IActionResult GetHistory(int id) =>
        Ok(_service.GetHistory(id, CurrentUserId, CurrentRole));
}
