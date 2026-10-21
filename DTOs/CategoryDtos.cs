using System.ComponentModel.DataAnnotations;
using ProductApi.Models;

namespace ProductApi.DTOs;

public class CategoryRequest
{
    [Required(ErrorMessage = "නම අනිවාර්යයි")]
    [StringLength(60, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [StringLength(250)]
    public string Description { get; set; } = "";

    [Required] public TicketPriority DefaultPriority { get; set; } = TicketPriority.Medium;

    [Range(1, 2160, ErrorMessage = "SLA පැය 1-2160 අතර")]
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
