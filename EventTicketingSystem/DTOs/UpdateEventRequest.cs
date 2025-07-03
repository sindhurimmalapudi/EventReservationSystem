using System.ComponentModel.DataAnnotations;

public class UpdateEventRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public TicketTypeRequest[]? TicketTypes { get; set; }   
}