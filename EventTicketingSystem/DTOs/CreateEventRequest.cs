using System.ComponentModel.DataAnnotations;

public class CreateEventRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public Guid VenueId { get; set; }

    [Required]
    [MinLength(1)]
    public List<TicketTypeRequest> TicketTypes { get; set; } = new();
}