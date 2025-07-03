using System.ComponentModel.DataAnnotations;

public class ReserveTicketRequest
{
    [Required]
    public Guid EventId { get; set; }
    [Required]
    public Guid TicketTypeId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; } = 1; // Default to 1 ticket if not specified
}