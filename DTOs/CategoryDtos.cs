using System.ComponentModel.DataAnnotations;
using HelpdeskApi.Models;

namespace HelpdeskApi.DTOs;

public class CategoryRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(60, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [StringLength(250)]
    public string Description { get; set; } = "";

    [Required] public TicketPriority DefaultPriority { get; set; } = TicketPriority.Medium;

    [Range(1, 2160, ErrorMessage = "SLA hours must be between 1 and 2160")]
    public int SlaHours { get; set; } = 24;
}

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string DefaultPriority { get; set; } = "";
    public int SlaHours { get; set; }
    public int TicketCount { get; set; }
}
