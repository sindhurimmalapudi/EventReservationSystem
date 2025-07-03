using System.ComponentModel.DataAnnotations;

public class TicketTypeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty; // e.g., "VIP", "General"
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
    public int Capacity { get; set; }
}